using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Pseudo;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace SilverPinesTranslate
{
    [BepInPlugin(Guid, "Silver Pines Community Translate", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.michal.silverpines.translate";

        internal static ManualLogSource Log;
        // tableCollectionName -> (keyId -> polish target)
        internal static readonly Dictionary<string, Dictionary<long, string>> Map =
            new Dictionary<string, Dictionary<long, string>>(StringComparer.Ordinal);
        // English source text -> Polish, for text that bypasses the localization tables (clip/voice subtitles).
        internal static readonly Dictionary<string, string> SourceMap = new Dictionary<string, string>(StringComparer.Ordinal);
        // Voice subtitle mappings (not in main StringTables, added upfront).
        private static readonly Dictionary<string, string> VoiceSubtitles = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { NormKey("The heat is much too intense."), "Temperatura jest zbyt wysoka." },
            { NormKey("Evacuation order in effect."), "Rozkaz ewakuacji w mocy." }
        };
        internal static int Hits;
        internal static readonly HashSet<string> SeenTables = new HashSet<string>(StringComparer.Ordinal);
        internal static TMP_FontAsset SerifPL, TypePL;
        private static readonly char[] PlChars = "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ".ToCharArray();
        private const float POLISH_SCALE = 1.15f;
        internal static string NoticiaTtf, CutiveTtf;
        private static readonly HashSet<int> _converted = new HashSet<int>();
        private static readonly HashSet<string> _seenFam = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static FieldInfo _srcPathField, _srcFontField;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("=== Silver Pines Community Translate 0.1.0 ===");
            LoadTranslations();
            try { new Harmony(Guid).PatchAll(Assembly.GetExecutingAssembly()); Log.LogInfo("Harmony patched."); }
            catch (Exception e) { Log.LogError("Harmony patch failed: " + e); }
            StartCoroutine(Setup());
        }

        private void LoadTranslations()
        {
            try
            {
                string baseDir = Path.GetDirectoryName(Info.Location);
                string dir = Path.Combine(baseDir, "Translations");
                if (!Directory.Exists(dir)) { Log.LogWarning("No Translations folder at " + dir); return; }
                int total = 0;
                foreach (var file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
                {
                    JObject root;
                    try { root = JObject.Parse(File.ReadAllText(file)); }
                    catch (Exception ex) { Log.LogWarning($"Skip {Path.GetFileName(file)}: {ex.Message}"); continue; }
                    if (!(root["tables"] is JObject tables)) continue;
                    foreach (var tbl in tables)
                    {
                        if (!(tbl.Value is JObject entries)) continue;
                        if (!Map.TryGetValue(tbl.Key, out var d)) { d = new Dictionary<long, string>(); Map[tbl.Key] = d; }
                        foreach (var e in entries)
                        {
                            if (!long.TryParse(e.Key, out var keyId)) continue;
                            string tgt = e.Value?["target"]?.ToString();
                            if (string.IsNullOrEmpty(tgt)) continue;
                            d[keyId] = tgt; total++;
                            string src = e.Value?["source"]?.ToString();
                            if (!string.IsNullOrEmpty(src)) SourceMap[NormKey(src)] = tgt;
                        }
                    }
                }
                // Load voice subtitles (from ScriptableObject clips, not the main StringTables).
                foreach (var kv in VoiceSubtitles) SourceMap[kv.Key] = kv.Value;
                Log.LogInfo($"Loaded {total} translations across {Map.Count} tables: {string.Join(", ", new List<string>(Map.Keys).ToArray())}; +{VoiceSubtitles.Count} voice subtitles");
            }
            catch (Exception e) { Log.LogError("LoadTranslations failed: " + e); }
        }

        private IEnumerator Setup()
        {
            // wait for the localization system to finish initializing
            var init = LocalizationSettings.InitializationOperation;
            float t = 0f;
            while (!init.IsDone && t < 30f) { t += Time.unscaledDeltaTime; yield return null; }
            Log.LogInfo($"Localization init done = {init.IsDone} after {t:F1}s");

            // Register the official table postprocessor: every string table gets ALL its entries rewritten to
            // Polish on load. This covers text read directly from the entry value (radio/TV broadcasts, etc.)
            // that bypasses GetLocalizedString. Also apply to already-loaded tables right now.
            try
            {
                var db = LocalizationSettings.StringDatabase;
                db.TablePostprocessor = new PolishTablePostprocessor(db.TablePostprocessor);
                int applied = 0, tables = 0;
                foreach (var st in Resources.FindObjectsOfTypeAll<StringTable>()) { int k = ApplyToTable(st); if (k > 0) { applied += k; tables++; } }
                Log.LogInfo($"TablePostprocessor registered; applied to {applied} entries in {tables} already-loaded tables.");
            }
            catch (Exception e) { Log.LogError("Postprocessor setup failed: " + e); }

            // Force-load EVERY translated table now, so each is rewritten to Polish UPFRONT. This covers text
            // that is read directly from the table (item/examine descriptions, radio, TV) and might otherwise
            // be displayed in English before a per-string hook or the periodic sweep catches it.
            try
            {
                int n = 0;
                foreach (var tableName in new List<string>(Map.Keys))
                {
                    var op = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
                    op.Completed += h => { try { ApplyToTable(h.Result); } catch { } };
                    n++;
                }
                Log.LogInfo($"Force-loading {n} string tables for upfront PL rewrite.");
            }
            catch (Exception e) { Log.LogError("Preload tables failed: " + e); }

            // wait until TMP is ready (TMP_Settings.instance lazy-loads from Resources on first access)
            float t2 = 0f;
            while (TMP_Settings.instance == null && t2 < 20f) { t2 += Time.unscaledDeltaTime; yield return null; }
            Log.LogInfo($"TMP_Settings.instance ready = {TMP_Settings.instance != null} after {t2:F1}s");
            EnsureFallbackFont();

            // give the menu a moment, then refresh visible localized text so it re-resolves (and re-renders with the fallback font)
            yield return new WaitForSeconds(1.0f);
            RefreshVisible();
            SwapComponentFonts();
            yield return new WaitForSeconds(2.0f);
            RefreshVisible();
            SwapComponentFonts();
            Log.LogInfo($"Setup complete. Translation hits so far = {Hits}. Tables observed: {string.Join(", ", new List<string>(SeenTables).ToArray())}");

            // Keep re-applying to string tables that load LATER during gameplay (radio, TV, objectives, new
            // areas). Cheap: ApplyToTable is a no-op once an entry is already Polish. Refresh only when something
            // actually changed, to avoid flicker.
            while (true)
            {
                yield return new WaitForSeconds(2f);
                int applied = 0;
                try { foreach (var st in Resources.FindObjectsOfTypeAll<StringTable>()) applied += ApplyToTable(st); } catch { }
                SwapComponentFonts();
                if (applied > 0)
                {
                    Log.LogInfo($"[periodic] applied {applied} entries -> PL (new content loaded).");
                    RefreshVisible();
                }
            }
        }

        private void EnsureFallbackFont()
        {
            try
            {
                if (TMP_Settings.instance == null) { Log.LogWarning("TMP_Settings.instance still null; skipping fallback font."); return; }
                string fontsDir = Path.Combine(Path.GetDirectoryName(Info.Location), "fonts");

                // Build dynamic TMP fonts straight from the shipped TTF files (each rasterizes Polish glyphs on demand).
                string cutivePath  = Path.Combine(fontsDir, "CutiveMono-Regular.ttf");
                string noticiaPath = Path.Combine(fontsDir, "NoticiaText-Regular.ttf");

                var all = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

                // Diagnostic: full face metrics of every loaded font (to verify/tune sizing).
                foreach (var fa in all)
                {
                    var fi = fa.faceInfo;
                    Log.LogInfo($"[font] '{fa.name}' [{fi.familyName}/{fi.styleName}] pop={fa.atlasPopulationMode} pt={fi.pointSize} sc={fi.scale} cap={fi.capLine:F1} mean={fi.meanLine:F1} asc={fi.ascentLine:F1}");
                }

                // IMPORTANT: do NOT ClearFontAssetData/convert the game's LIVE fonts (calling that from text
                // hooks mid-frame crashes the game). Instead we SWAP Polish-containing components to our own
                // pre-built dynamic fonts (SerifPL/TypePL @ POLISH_SCALE) — safe, immediate, and was rated 10/10.
                NoticiaTtf = noticiaPath; CutiveTtf = cutivePath;
                int converted = 0;

                // 2) FALLBACK fonts for typefaces with no Polish source (Special Elite typewriter -> Cutive Mono),
                //    plus a Noticia serif fallback as a safety net if (1) didn't take effect.
                var typewriter = LoadDynamic(cutivePath, "SP_Typewriter_PL");
                var serif      = LoadDynamic(noticiaPath, "SP_Serif_PL");
                TypePL = typewriter; SerifPL = serif; // used by SwapComponentFonts

                // Pick a representative game font per family to copy size metrics from, so fallback glyphs
                // (ą ę ł …) render at the SAME visual size as the primary font, not smaller.
                TMP_FontAsset repType = null, repSerif = null;
                foreach (var fa in all)
                {
                    if (fa == typewriter || fa == serif) continue;
                    string fam = fa.faceInfo.familyName ?? "";
                    if (repType == null && (fam.IndexOf("Special Elite", StringComparison.OrdinalIgnoreCase) >= 0 || fam.IndexOf("Typewriter", StringComparison.OrdinalIgnoreCase) >= 0)) repType = fa;
                    if (repSerif == null && fam.IndexOf("Noticia", StringComparison.OrdinalIgnoreCase) >= 0) repSerif = fa;
                }
                if (typewriter != null && repType != null) MatchSize(typewriter, repType);
                if (serif != null && repSerif != null) MatchSize(serif, repSerif);

                // Global catch-all (typewriter first — most UI uses the Special Elite typewriter font).
                var glob = TMP_Settings.fallbackFontAssets;
                if (glob != null)
                {
                    if (serif != null) { glob.Remove(serif); glob.Insert(0, serif); }
                    if (typewriter != null) { glob.Remove(typewriter); glob.Insert(0, typewriter); } // typewriter ends up first
                }

                // Per-family LOCAL fallbacks (TMP checks these BEFORE the global list) so each game font
                // gets a typeface-matched Polish fallback: Special Elite -> typewriter, Noticia -> serif.
                int attached = 0;
                foreach (var fa in all)
                {
                    if (fa == typewriter || fa == serif) continue;
                    string fam = fa.faceInfo.familyName ?? "";
                    TMP_FontAsset match = null;
                    if (fam.IndexOf("Special Elite", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        fam.IndexOf("Typewriter", StringComparison.OrdinalIgnoreCase) >= 0) match = typewriter;
                    else if (fam.IndexOf("Noticia", StringComparison.OrdinalIgnoreCase) >= 0) match = serif;
                    if (match == null) continue;
                    AddLocalFallback(fa, match);
                    attached++;
                }

                // Swap fonts render the WHOLE Polish-containing component (Latin+PL uniformly from one font).
                // Bump their scale so they match (or slightly exceed) the game's baked text — tunable.
                if (serif != null) { var f1 = serif.faceInfo; f1.scale = POLISH_SCALE; serif.faceInfo = f1; }
                if (typewriter != null) { var f2 = typewriter.faceInfo; f2.scale = POLISH_SCALE; typewriter.faceInfo = f2; }
                Log.LogInfo($"Font fallbacks ready. converted={converted} per-family attached={attached} POLISH_SCALE={POLISH_SCALE}");
            }
            catch (Exception e) { Log.LogError("EnsureFallbackFont failed: " + e); }
        }

        // Scale the fallback font so its cap-height-per-em matches the primary, making ą/ę/ł render at the
        // same size as the surrounding text. Logs metrics so the result can be verified/tuned.
        private void MatchSize(TMP_FontAsset fb, TMP_FontAsset primary)
        {
            try
            {
                var p = primary.faceInfo;
                var f = fb.faceInfo;
                float pCap = p.capLine != 0f ? p.capLine : p.ascentLine;
                float fCap = f.capLine != 0f ? f.capLine : f.ascentLine;
                float pSc = p.scale == 0f ? 1f : p.scale;
                float fSc = f.scale == 0f ? 1f : f.scale;
                float pNorm = (p.pointSize > 0 && pCap != 0f) ? (pCap / p.pointSize) * pSc : 0f;
                float fNorm = (f.pointSize > 0 && fCap != 0f) ? (fCap / f.pointSize) * fSc : 0f;
                Log.LogInfo($"[size] primary '{primary.name}' pt={p.pointSize} sc={p.scale} cap={p.capLine} asc={p.ascentLine} norm={pNorm:F4} || fb '{fb.name}' pt={f.pointSize} sc={f.scale} cap={f.capLine} asc={f.ascentLine} norm={fNorm:F4}");
                if (pNorm > 0f && fNorm > 0f)
                {
                    float ratio = pNorm / fNorm;
                    f.scale = fSc * ratio;
                    fb.faceInfo = f;
                    Log.LogInfo($"[size] '{fb.name}' scale x{ratio:F3} -> {f.scale:F4}");
                }
            }
            catch (Exception e) { Log.LogWarning("MatchSize failed: " + e); }
        }

        private TMP_FontAsset LoadDynamic(string path, string name)
        {
            try
            {
                if (!File.Exists(path)) { Log.LogWarning("Font file missing: " + path); return null; }
                var fa = TMP_FontAsset.CreateFontAsset(path, 0, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024); // loads from file, Dynamic
                if (fa == null) { Log.LogWarning("CreateFontAsset(file) returned null for " + Path.GetFileName(path)); return null; }
                fa.name = name;
                Log.LogInfo($"Loaded dynamic font '{name}' from {Path.GetFileName(path)} (family={fa.faceInfo.familyName}).");
                return fa;
            }
            catch (Exception e) { Log.LogError($"LoadDynamic({name}) failed: " + e); return null; }
        }

        private static void AddLocalFallback(TMP_FontAsset fa, TMP_FontAsset fb)
        {
            var list = fa.fallbackFontAssetTable;
            if (list == null)
            {
                list = new List<TMP_FontAsset>();
                AccessTools.Field(typeof(TMP_FontAsset), "m_FallbackFontAssetTable")?.SetValue(fa, list);
            }
            list.Remove(fb);      // put ours FIRST, so it is used before the game's own (Noto) fallbacks
            list.Insert(0, fb);
        }

        private void RefreshVisible()
        {
            try
            {
                int n = 0;
                foreach (var ev in Resources.FindObjectsOfTypeAll<LocalizeStringEvent>()) { ev.RefreshString(); n++; }
                var fonts = new HashSet<string>(StringComparer.Ordinal);
                foreach (var tmp in Resources.FindObjectsOfTypeAll<TMP_Text>())
                {
                    tmp.ForceMeshUpdate(true);
                    var f = tmp.font;
                    if (f != null) fonts.Add($"{f.name} [{f.faceInfo.familyName}/{f.faceInfo.styleName}]");
                }
                Log.LogInfo($"RefreshVisible: refreshed {n} LocalizeStringEvent(s). On-screen TMP fonts: {string.Join(" | ", new List<string>(fonts).ToArray())}");
            }
            catch (Exception e) { Log.LogWarning("RefreshVisible failed: " + e); }
        }

        // Swap the FONT of any text component that contains Polish characters to our Polish-capable font
        // (Noticia for everything, Cutive Mono for the Special Elite typewriter). This makes the whole string
        // render in one font that HAS Polish glyphs — no fallback, no size/typeface mismatch. Idempotent.
        private void SwapComponentFonts()  // periodic safety-net sweep
        {
            if (SerifPL == null && TypePL == null) return;
            try { foreach (var tmp in Resources.FindObjectsOfTypeAll<TMP_Text>()) SwapOne(tmp); } catch { }
        }

        // Swap ONE component's font to a Polish-capable one if its text needs Polish glyphs. Idempotent.
        internal static bool SwapOne(TMP_Text tmp)
        {
            if (tmp == null || (SerifPL == null && TypePL == null)) return false;
            var f = tmp.font;
            if (f == null || f == SerifPL || f == TypePL) return false;
            string txt = tmp.text;
            if (string.IsNullOrEmpty(txt) || txt.IndexOfAny(PlChars) < 0) return false; // only text needing PL glyphs
            string fam = f.faceInfo.familyName ?? "";
            TMP_FontAsset repl = (fam.IndexOf("Special Elite", StringComparison.OrdinalIgnoreCase) >= 0) ? TypePL : SerifPL;
            if (repl == null) return false;
            try { var col = tmp.color; tmp.font = repl; tmp.color = col; Log.LogInfo($"[swap] '{tmp.name}' fam='{fam}' -> {repl.name}"); return true; } catch { return false; }
        }

        // Convert ONE game font asset (Noticia / Special Elite) to render Polish itself at its own size. Called the
        // instant a component's text is set, so screens whose font loads on open are fixed with no delay. Idempotent.
        internal static void ConvertOneFont(TMP_FontAsset fa)
        {
            if (fa == null || fa == SerifPL || fa == TypePL || string.IsNullOrEmpty(NoticiaTtf)) return;
            if (!_converted.Add(fa.GetInstanceID())) return; // already handled
            if (_srcPathField == null)
            {
                _srcPathField = AccessTools.Field(typeof(TMP_FontAsset), "m_SourceFontFilePath");
                _srcFontField = AccessTools.Field(typeof(TMP_FontAsset), "m_SourceFontFile");
            }
            string fam = fa.faceInfo.familyName ?? "";
            bool isNoticia = fam.IndexOf("Noticia", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isSpecial = fam.IndexOf("Special Elite", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isNoticia && !isSpecial)
            {
                if (_seenFam.Add(fam)) Log?.LogInfo($"[font-other] '{fa.name}' family='{fam}'"); // diagnostic for non-converted fonts
                return;
            }
            string ttf = isNoticia ? NoticiaTtf : CutiveTtf;
            if (!File.Exists(ttf)) return;
            try
            {
                _srcFontField?.SetValue(fa, null);
                _srcPathField?.SetValue(fa, ttf);
                fa.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                if (isNoticia) fa.ClearFontAssetData(false);   // re-rasterize ALL glyphs (Latin too) -> uniform size
                fa.ReadFontAssetDefinition();
                if (isNoticia) { var fi = fa.faceInfo; fi.scale = POLISH_SCALE; fa.faceInfo = fi; }
                Log?.LogInfo($"[fontconv] {fa.name} ({fam}){(isNoticia ? " cleared+scaled" : "")}");
            }
            catch (Exception ex) { Log?.LogWarning($"fontconv {fa.name}: {ex.Message}"); }
        }

        internal static int ConvertFonts()
        {
            int before = _converted.Count;
            try { foreach (var fa in Resources.FindObjectsOfTypeAll<TMP_FontAsset>()) ConvertOneFont(fa); } catch { }
            return _converted.Count - before;
        }

        internal static bool Lookup(string table, long keyId, out string pl)
        {
            pl = null;
            if (string.IsNullOrEmpty(table)) return false;
            if (Map.TryGetValue(table, out var d) && d.TryGetValue(keyId, out pl)) return true;
            int us = table.LastIndexOf('_');
            if (us > 0 && Map.TryGetValue(table.Substring(0, us), out var d2) && d2.TryGetValue(keyId, out pl)) return true;
            return false;
        }

        internal static string NormKey(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

        // Translate a raw English string by exact source-text match (for text outside the localization tables).
        internal static bool TranslateText(string english, out string pl)
        {
            pl = null;
            return !string.IsNullOrEmpty(english) && SourceMap.TryGetValue(NormKey(english), out pl);
        }

        // Rewrite every translated entry's raw value to Polish so ALL read paths (GetLocalizedString, Value,
        // LocalizedValue, direct) return Polish. Returns the number of entries changed.
        internal static int ApplyToTable(StringTable st)
        {
            if (st == null) return 0;
            if (!Map.TryGetValue(st.TableCollectionName, out var d)) return 0;
            int n = 0;
            foreach (var e in st.Values)
            {
                if (e != null && d.TryGetValue(e.KeyId, out var pl) && e.Value != pl) { e.Value = pl; n++; }
            }
            return n;
        }
    }

    // Voice / TV / radio clip subtitles come from ScriptableObjects (ClipSubtitlesAsset), NOT the localization
    // tables, and are pushed through SubtitlesDisplay.SubtitlesEvent (m_text.text = englishSubtitle). Translate
    // the displayed English text by exact source-text match.
    [HarmonyPatch]
    internal static class SubtitlesPatch
    {
        static System.Collections.Generic.IEnumerable<MethodBase> TargetMethods()
        {
            var t = AccessTools.TypeByName("SubtitlesDisplay");
            var m = t != null ? AccessTools.Method(t, "SubtitlesEvent") : null;
            if (m != null) yield return m;
        }
        static void Postfix(object __instance)
        {
            try
            {
                var tmp = AccessTools.Field(__instance.GetType(), "m_text")?.GetValue(__instance) as TMP_Text;
                if (tmp != null && Plugin.TranslateText(tmp.text, out var pl)) tmp.text = pl;
            }
            catch { }
        }
    }

    // Swap the font the instant the game assigns a component's text, so a Polish text never flashes in the
    // wrong font before the periodic sweep catches it.
    [HarmonyPatch]
    internal static class TmpTextSetterPatch
    {
        static MethodBase TargetMethod() => AccessTools.PropertySetter(typeof(TMP_Text), "text");
        static void Postfix(TMP_Text __instance) { try { Plugin.SwapOne(__instance); } catch { } }
    }

    // Also catch text assigned via SetText(...) overloads (some UI uses these instead of the text setter).
    [HarmonyPatch]
    internal static class TmpSetTextPatch
    {
        static System.Collections.Generic.IEnumerable<MethodBase> TargetMethods()
        {
            var m1 = AccessTools.Method(typeof(TMP_Text), "SetText", new[] { typeof(string) });
            if (m1 != null) yield return m1;
            var m2 = AccessTools.Method(typeof(TMP_Text), "SetText", new[] { typeof(string), typeof(bool) });
            if (m2 != null) yield return m2;
        }
        static void Postfix(TMP_Text __instance) { try { Plugin.SwapOne(__instance); } catch { } }
    }

    // Official Unity Localization modding hook: fires once per string-table load.
    internal class PolishTablePostprocessor : UnityEngine.Localization.Settings.ITablePostprocessor
    {
        private readonly UnityEngine.Localization.Settings.ITablePostprocessor _inner;
        public PolishTablePostprocessor(UnityEngine.Localization.Settings.ITablePostprocessor inner) { _inner = inner; }

        public void PostprocessTable(LocalizationTable table)
        {
            try { _inner?.PostprocessTable(table); } catch { }
            try
            {
                var st = table as StringTable;
                int n = Plugin.ApplyToTable(st);
                if (n > 0) Plugin.Log.LogInfo($"[postproc] {st.TableCollectionName}: {n} -> PL");
            }
            catch { }
        }
    }

    // Inject Polish at the ENTRY level, BEFORE smart-string / String.Format runs.
    // StringTableEntry.GetLocalizedString reads the raw template (Data.Localized) then substitutes {0}/{1}.
    // Patching it covers BOTH the database path (LocalizedStringDatabase.GenerateLocalizedString calls this)
    // AND any direct table-entry access (item names, etc.). Setting Value = our Polish template lets the
    // game apply its own {0}/{1} arguments and smart formatting, so placeholders fill correctly.
    [HarmonyPatch]
    internal static class StringTableEntryPatch
    {
        static MethodBase TargetMethod() =>
            AccessTools.Method("UnityEngine.Localization.Tables.StringTableEntry:GetLocalizedString",
                new Type[] { typeof(IFormatProvider), typeof(IList<object>), typeof(PseudoLocale) });

        static void Prefix(StringTableEntry __instance)
        {
            try
            {
                var table = __instance.Table;
                if (table == null) return;
                string name = table.TableCollectionName;
                if (Plugin.SeenTables.Add(name)) Plugin.Log.LogInfo($"[table] '{name}'");
                if (Plugin.Lookup(name, __instance.KeyId, out var pl) && __instance.Value != pl)
                {
                    __instance.Value = pl; // raw template; game then fills {0}/{1} + smart formatting
                    Plugin.Hits++;
                }
            }
            catch { /* never break the game over a translation */ }
        }
    }
}
