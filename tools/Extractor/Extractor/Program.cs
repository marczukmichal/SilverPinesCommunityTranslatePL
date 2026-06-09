using AssetsTools.NET;
using AssetsTools.NET.Extra;

string bw = @"C:\Program Files (x86)\Steam\steamapps\common\Silver Pines Demo\Carpenter_Data\StreamingAssets\aa\StandaloneWindows64";
string bundlePath = Path.Combine(bw, "fonts_assets_all.bundle");
Console.WriteLine($"Loading {Path.GetFileName(bundlePath)} ({new FileInfo(bundlePath).Length / 1024 / 1024} MB)");

var am = new AssetsManager();
var bun = am.LoadBundleFile(bundlePath);
var inst = am.LoadAssetsFileFromBundle(bun, 0, false);
var file = inst.file;
Console.WriteLine($"Assets in file: {file.AssetInfos.Count}");

foreach (var info in file.AssetInfos)
{
    AssetTypeValueField bf;
    try { bf = am.GetBaseField(inst, info); } catch { continue; }
    var face = bf.Children?.FirstOrDefault(c => c.FieldName == "m_FaceInfo");
    if (face == null) continue; // only TMP_FontAsset has m_FaceInfo

    string name = "", fam = "", style = "";
    int pop = -1, chars = 0;
    try { name = bf["m_Name"].AsString; } catch { }
    try { fam = face["m_FamilyName"].AsString; } catch { }
    try { style = face["m_StyleName"].AsString; } catch { }
    try { pop = bf["m_AtlasPopulationMode"].AsInt; } catch { }
    try { chars = bf["m_CharacterTable"]["Array"].Children.Count; } catch { }
    string popName = pop == 0 ? "Dynamic" : pop == 1 ? "Static" : pop.ToString();
    Console.WriteLine($"FONT name='{name}'  family='{fam}'  style='{style}'  atlasPop={popName}  glyphs={chars}");
}
Console.WriteLine("done");
