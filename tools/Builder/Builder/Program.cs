using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

string root = @"C:\Users\Michal\Documents\subtitles-mod-game";
string srcPath = Path.Combine(root, "build", "english-source.json");
string workDir = Path.Combine(root, "Translations", "pl", "_work");
string manualPath = Path.Combine(root, "Translations", "pl", "targets.json");
string outPath = Path.Combine(root, "Translations", "pl", "Polski.json");

// --- load english source: table -> key -> english ---
var english = new Dictionary<string, Dictionary<string, string>>();
var srcDoc = JsonNode.Parse(File.ReadAllText(srcPath))!.AsObject();
foreach (var t in srcDoc)
{
    var d = new Dictionary<string, string>();
    foreach (var kv in t.Value!.AsObject()) d[kv.Key] = kv.Value!.GetValue<string>();
    english[t.Key] = d;
}

// --- collect targets: table -> key -> target ---
var targets = new Dictionary<string, Dictionary<string, string>>();
void Add(string table, string key, string val)
{
    if (!targets.TryGetValue(table, out var d)) { d = new(); targets[table] = d; }
    d[key] = val;
}

if (File.Exists(manualPath))
    foreach (var t in JsonNode.Parse(File.ReadAllText(manualPath))!.AsObject())
        if (!t.Key.StartsWith("_") && t.Value is JsonObject obj)
            foreach (var kv in obj) Add(t.Key, kv.Key, kv.Value!.GetValue<string>());

if (Directory.Exists(workDir))
    foreach (var dir in Directory.GetDirectories(workDir))
    {
        string table = Path.GetFileName(dir);
        foreach (var f in Directory.GetFiles(dir, "*.json"))
            foreach (var kv in JsonNode.Parse(File.ReadAllText(f))!.AsObject())
                if (!kv.Key.StartsWith("_")) Add(table, kv.Key, kv.Value!.GetValue<string>());
    }

// Manual corrections — read LAST so they override the AI translations.
string corrPath = Path.Combine(root, "Translations", "pl", "corrections.json");
if (File.Exists(corrPath))
    foreach (var t in JsonNode.Parse(File.ReadAllText(corrPath))!.AsObject())
        if (!t.Key.StartsWith("_") && t.Value is JsonObject obj)
            foreach (var kv in obj) Add(t.Key, kv.Key, kv.Value!.GetValue<string>());

// --- build + validate ---
var tagRx = new Regex("<[^>]+>");
var phRx = new Regex(@"\{[^}]*\}|%[sd]");
int totalSrc = 0, totalDone = 0, err = 0, warn = 0;
var coverage = new List<string>();
var issues = new List<string>();
var tablesNode = new JsonObject();

foreach (var table in english.Keys.OrderBy(x => x, StringComparer.Ordinal))
{
    var eng = english[table];
    targets.TryGetValue(table, out var tgt);
    int done = 0;
    var tnode = new JsonObject();
    foreach (var (key, en) in eng)
    {
        totalSrc++;
        if (tgt == null || !tgt.TryGetValue(key, out var pl)) continue;
        done++; totalDone++;
        bool tgtDash = pl.Contains('—') || pl.Contains('–');
        bool srcDash = en.Contains('—') || en.Contains('–');
        if (tgtDash && !srcDash)
        { issues.Add($"  [ERR ] invented dash in {table}/{key}: {Trunc(pl)}"); err++; }
        if (!Eq(Tokens(tagRx, en), Tokens(tagRx, pl)))
        { issues.Add($"  [WARN] tag mismatch {table}/{key}"); warn++; }
        if (!Eq(Tokens(phRx, en), Tokens(phRx, pl)))
        { issues.Add($"  [WARN] placeholder mismatch {table}/{key}"); warn++; }
        if (CountNl(en) != CountNl(pl))
        { issues.Add($"  [WARN] newline count {table}/{key}: EN={CountNl(en)} PL={CountNl(pl)}"); warn++; }
        if (pl == en && en.Length > 3)
        { issues.Add($"  [INFO] identical to EN {table}/{key}: {Trunc(pl)}"); }
        tnode[key] = new JsonObject { ["source"] = en, ["target"] = pl, ["ai"] = false };
    }
    if (done > 0) tablesNode[table] = tnode;
    coverage.Add($"  {table,-20} {done,5} / {eng.Count}");
}

var outObj = new JsonObject
{
    ["meta"] = new JsonObject { ["language"] = "pl", ["languageName"] = "Polski", ["baseLanguage"] = "en", ["version"] = 1 },
    ["tables"] = tablesNode
};
var opts = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
File.WriteAllText(outPath, outObj.ToJsonString(opts), new UTF8Encoding(false));

Console.WriteLine("Coverage per table:");
foreach (var c in coverage) Console.WriteLine(c);
Console.WriteLine($"\nTOTAL: {totalDone}/{totalSrc} ({100.0 * totalDone / totalSrc:F1}%)");
Console.WriteLine($"Validation: {err} errors, {warn} warnings");
foreach (var i in issues) Console.WriteLine(i);
Console.WriteLine($"\nWrote {outPath}");

// Emit per-scene chunk files of UNtranslated lines (in game/scene order) for the translation workflow.
if (args.Length > 0 && args[0] == "chunks")
{
    var skip = new HashSet<string>(StringComparer.Ordinal) { "Save" };
    string chunkDir = Path.Combine(root, "build", "chunks");
    if (Directory.Exists(chunkDir)) Directory.Delete(chunkDir, true);
    Directory.CreateDirectory(chunkDir);
    const int chunkSize = 45;
    var manifest = new JsonArray();
    foreach (var t in srcDoc)
    {
        string table = t.Key;
        if (skip.Contains(table)) continue;
        targets.TryGetValue(table, out var done);
        var missing = new List<KeyValuePair<string, string>>();
        foreach (var kv in t.Value!.AsObject())
            if (done == null || !done.ContainsKey(kv.Key))
                missing.Add(new KeyValuePair<string, string>(kv.Key, kv.Value!.GetValue<string>()));
        for (int i = 0; i < missing.Count; i += chunkSize)
        {
            int len = Math.Min(chunkSize, missing.Count - i);
            var lines = new JsonObject();
            for (int j = i; j < i + len; j++) lines[missing[j].Key] = missing[j].Value;
            var chunkObj = new JsonObject { ["table"] = table, ["lines"] = lines };
            string baseName = $"{table}__{i:D4}";
            File.WriteAllText(Path.Combine(chunkDir, baseName + ".json"), chunkObj.ToJsonString(opts), new UTF8Encoding(false));
            manifest.Add(baseName);
        }
    }
    File.WriteAllText(Path.Combine(root, "build", "chunks_manifest.json"), manifest.ToJsonString(opts), new UTF8Encoding(false));
    Console.WriteLine($"Emitted {manifest.Count} chunk files (chunkSize={chunkSize}) to {chunkDir}");
}

static string Trunc(string s) => s.Length > 55 ? s[..55] + "..." : s;
static int CountNl(string s) { int c = 0; foreach (var ch in s) if (ch == '\n') c++; return c; }
static List<string> Tokens(Regex rx, string s)
{
    var l = new List<string>();
    foreach (Match m in rx.Matches(s)) l.Add(m.Value);
    l.Sort(StringComparer.Ordinal);
    return l;
}
static bool Eq(List<string> a, List<string> b)
{
    if (a.Count != b.Count) return false;
    for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
    return true;
}
