using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SanitariumGreekPatcher;

public static class Program
{
    const string Version = "1.0";
    const string BackupDir = "GreekPatch_Backup";
    static bool pause = true;
    static string Md5(byte[] b) => Convert.ToHexString(MD5.HashData(b)).ToLowerInvariant();
    static void Log(string s) => Console.WriteLine(s);
    static void Fail(string s) => throw new PatchError(s);
    sealed class PatchError : Exception { public PatchError(string m) : base(m) { } }

    public static int Main(string[] args)
    {
        try { Console.OutputEncoding = new UTF8Encoding(false); } catch { }
        string game = null, dataDir = null; bool restore = false;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--game": game = args[++i]; break;
                case "--data": dataDir = args[++i]; break;
                case "--restore": restore = true; break;
                case "--no-pause": pause = false; break;
                default: Log($"Άγνωστη επιλογή / unknown option: {args[i]}"); return 2;
            }
        }
        int rc = 0;
        try
        {
            Log($"=== Sanitarium — Ελληνική μετάφραση v{Version} / Greek translation ===");
            string root = FindGameRoot(game);
            Log($"Φάκελος παιχνιδιού / game folder: {root}");
            var man = Manifest.Load(dataDir ?? FindData());
            if (restore) Restore(root, man); else Apply(root, man);
        }
        catch (PatchError e) { Console.Error.WriteLine("\nΣΦΑΛΜΑ / ERROR: " + e.Message); rc = 1; }
        catch (Exception e) { Console.Error.WriteLine("\nΑΠΡΟΣΔΟΚΗΤΟ ΣΦΑΛΜΑ / UNEXPECTED ERROR: " + e); rc = 3; }
        if (pause) { Log("\nΠάτα Enter για έξοδο / press Enter to exit..."); try { Console.ReadLine(); } catch { } }
        return rc;
    }

    // ---------------------------------------------------------------- locating things
    static string FindData()
    {
        foreach (var d in new[] { Path.Combine(AppContext.BaseDirectory, "Data"), Path.Combine(Directory.GetCurrentDirectory(), "Data"),
                                  Path.Combine(AppContext.BaseDirectory, "..", "Data") })
            if (File.Exists(Path.Combine(d, "manifest.json"))) return Path.GetFullPath(d);
        Fail("Δεν βρέθηκε ο φάκελος Data (manifest.json) δίπλα στο πρόγραμμα.\n" +
             "Κατέβασέ τον από τη σελίδα του mod στο Nexus και αποσυμπίεσέ τον στον ίδιο φάκελο με το πρόγραμμα, ώστε να υπάρχει το Data\\manifest.json.\n" +
             "Data folder not found: download it from the Nexus mod page and extract it next to this program (Data\\manifest.json).");
        return null;
    }

    static bool IsGameRoot(string d) => File.Exists(Path.Combine(d, "Data", "EN", "RES.000")) && File.Exists(Path.Combine(d, "Data", "EN", "RES.001"));

    static string FindGameRoot(string given)
    {
        if (given != null)
        {
            if (!IsGameRoot(given)) Fail($"Ο φάκελος '{given}' δεν μοιάζει με το Sanitarium (λείπει Data\\EN\\RES.000).");
            return Path.GetFullPath(given);
        }
        var cands = new List<string> { Directory.GetCurrentDirectory(), AppContext.BaseDirectory, Path.Combine(AppContext.BaseDirectory, "..") };
        string home = Environment.GetEnvironmentVariable("HOME") ?? "";
        cands.Add(Path.Combine(home, ".local/share/Steam/steamapps/common/Sanitarium"));
        cands.Add(Path.Combine(home, ".steam/steam/steamapps/common/Sanitarium"));
        foreach (var drv in new[] { "C", "D", "E", "F", "G", "H" })
        {
            foreach (var sub in new[] { @"Program Files (x86)\Steam", @"Program Files\Steam", "Steam", "SteamLibrary", @"Games\Steam", @"Games\SteamLibrary" })
                cands.Add($@"{drv}:\{sub}\steamapps\common\Sanitarium");
            foreach (var sub in new[] { @"GOG Games\Sanitarium", @"Program Files (x86)\GOG Galaxy\Games\Sanitarium", @"GOG Galaxy\Games\Sanitarium", @"Games\Sanitarium" })
                cands.Add($@"{drv}:\{sub}");
        }
        foreach (var c in cands) { try { if (IsGameRoot(c)) return Path.GetFullPath(c); } catch { } }
        Log("Δεν βρέθηκε αυτόματα το παιχνίδι. Γράψε τον φάκελο εγκατάστασης\n(π.χ. C:\\Program Files (x86)\\Steam\\steamapps\\common\\Sanitarium):");
        string p = Console.ReadLine()?.Trim().Trim('"');
        if (string.IsNullOrEmpty(p) || !IsGameRoot(p)) Fail("Μη έγκυρος φάκελος παιχνιδιού.");
        return Path.GetFullPath(p);
    }

    static string P(string root, string rel) => Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
    static string B(string root, string rel) => Path.Combine(root, BackupDir, rel.Replace('/', Path.DirectorySeparatorChar));

    // ---------------------------------------------------------------- RES pack: u32 count, u32 offsets[count], data
    static List<byte[]> ReadPack(byte[] d)
    {
        uint n = BitConverter.ToUInt32(d, 0);
        if (n == 0 || 4 + 4L * n > d.Length) Fail("Μη έγκυρο αρχείο RES / invalid RES pack.");
        var offs = new long[n];
        for (int i = 0; i < n; i++) offs[i] = BitConverter.ToUInt32(d, 4 + 4 * i);
        var res = new List<byte[]>((int)n);
        for (int i = 0; i < n; i++)
        {
            long a = offs[i], b = i + 1 < n ? offs[i + 1] : d.Length;
            if (a > b || b > d.Length) Fail("Μη έγκυρο αρχείο RES / invalid RES pack.");
            res.Add(d[(int)a..(int)b]);
        }
        return res;
    }

    static byte[] WritePack(List<byte[]> ents)
    {
        int n = ents.Count; long size = 4 + 4L * n + ents.Sum(e => (long)e.Length);
        var outp = new byte[size]; BitConverter.GetBytes((uint)n).CopyTo(outp, 0);
        long pos = 4 + 4L * n;
        for (int i = 0; i < n; i++)
        {
            BitConverter.GetBytes((uint)pos).CopyTo(outp, 4 + 4 * i);
            ents[i].CopyTo(outp, pos); pos += ents[i].Length;
        }
        return outp;
    }

    // ---------------------------------------------------------------- file helpers
    static void WriteReplace(string dest, byte[] data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        string tmp = dest + ".greektmp"; File.WriteAllBytes(tmp, data);
        Exception last = null!;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                if (File.Exists(dest) && (File.GetAttributes(dest) & FileAttributes.ReadOnly) != 0) File.SetAttributes(dest, FileAttributes.Normal);
                File.Move(tmp, dest, true); return;
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is IOException) { last = e; Thread.Sleep(1500); }
        }
        try { File.Delete(tmp); } catch { }
        Fail($"Δεν μπόρεσα να γράψω το αρχείο:\n    {dest}\n" +
             "Κλείσε το παιχνίδι, το Steam/GOG Galaxy και τυχόν antivirus που σαρώνει τον φάκελο, και ξανατρέξε.\n" +
             "Could not write the file above. Close the game, Steam/GOG Galaxy and any antivirus scanning the folder, then run again.\n" +
             $"({last.GetType().Name}: {last.Message})");
    }

    // ---------------------------------------------------------------- apply / restore
    static void Apply(string root, Manifest man)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        // 1) έλεγχος όλων πριν αλλάξει οτιδήποτε / verify everything before touching anything
        var todo = new List<(Manifest.FileItem F, byte[] Src, bool NeedBackup)>();
        foreach (var f in man.Files)
        {
            string cur = P(root, f.Path), bk = B(root, f.Path);
            if (!File.Exists(cur)) Fail($"Λείπει το αρχείο του παιχνιδιού: {cur}\nΚάνε 'Verify integrity of game files' (Steam) ή επανεγκατάσταση.");
            var fi = new FileInfo(cur);
            if (fi.Length == f.NewSize && Md5(File.ReadAllBytes(cur)) == f.NewMd5) { Log($"  = {f.Path} (ήδη εγκατεστημένο / already patched)"); continue; }
            byte[] src; bool nb = false;
            if (File.Exists(bk))
            {
                src = File.ReadAllBytes(bk);
                if (Md5(src) != f.OrigMd5) Fail($"Το αντίγραφο ασφαλείας '{bk}' δεν είναι το αναμενόμενο αρχείο.\nΣβήσε τον φάκελο {BackupDir}, κάνε 'Verify integrity of game files' και ξανατρέξε.");
            }
            else
            {
                src = fi.Length == f.OrigSize ? File.ReadAllBytes(cur) : null;
                if (src == null || Md5(src) != f.OrigMd5)
                    Fail($"Το '{f.Path}' δεν είναι το αυθεντικό αρχείο της έκδοσης για την οποία φτιάχτηκε η μετάφραση\n" +
                         "(άλλη έκδοση παιχνιδιού ή ήδη τροποποιημένο από άλλο mod).\n" +
                         "Κάνε 'Verify integrity of game files' στο Steam (ή επανεγκατάσταση στο GOG) και ξανατρέξε.\n" +
                         $"'{f.Path}' is not the original file of the supported game version. Verify/reinstall the game and run again.");
                nb = true;
            }
            todo.Add((f, src, nb));
        }
        // 2) εφαρμογή / apply
        foreach (var (f, src, nb) in todo)
        {
            var ents = ReadPack(src);
            foreach (var kv in f.Entries)
            {
                int i = int.Parse(kv.Key);
                if (i < 0 || i >= ents.Count) Fail($"Μη αναμενόμενη δομή στο '{f.Path}' / unexpected pack layout.");
                ents[i] = Convert.FromBase64String(kv.Value);
            }
            byte[] outp = WritePack(ents);
            if (Md5(outp) != f.NewMd5) Fail($"Το αποτέλεσμα για το '{f.Path}' δεν είναι το αναμενόμενο.\nThe result for '{f.Path}' does not match the expected checksum.");
            if (nb) { string bk = B(root, f.Path); Directory.CreateDirectory(Path.GetDirectoryName(bk)!); File.WriteAllBytes(bk, src); }
            WriteReplace(P(root, f.Path), outp);
            Log($"  ✔ {f.Path}");
        }
        Log($"\n✔ Έτοιμο σε {sw.Elapsed.TotalSeconds:0}s! / Done!");
        Log($"Αντίγραφα ασφαλείας / backups: {Path.Combine(root, BackupDir)}");
        Log("Για απεγκατάσταση: Uninstall.bat (Linux: --restore) ή Steam → Verify integrity of game files.");
    }

    static void Restore(string root, Manifest man)
    {
        int restored = 0;
        foreach (var f in man.Files)
        {
            string bk = B(root, f.Path);
            if (!File.Exists(bk)) continue;
            byte[] b = File.ReadAllBytes(bk);
            if (Md5(b) != f.OrigMd5) Fail($"Το αντίγραφο ασφαλείας '{bk}' είναι κατεστραμμένο — κάνε 'Verify integrity of game files' στο Steam.");
            WriteReplace(P(root, f.Path), b); restored++;
            Log($"  ✔ {f.Path}");
        }
        Log($"✔ Επαναφέρθηκαν {restored} αρχεία / files restored.");
        if (restored == 0) Log("(Δεν βρέθηκαν αντίγραφα ασφαλείας — για πλήρη επαναφορά: Steam → Verify integrity of game files.)");
        else { try { Directory.Delete(Path.Combine(root, BackupDir), true); } catch { } }
    }
}

sealed class Manifest
{
    public sealed class FileItem
    {
        public string Path { get; set; } public string OrigMd5 { get; set; } public long OrigSize { get; set; }
        public string NewMd5 { get; set; } public long NewSize { get; set; } public Dictionary<string, string> Entries { get; set; }
    }
    public List<FileItem> Files { get; set; }

    public static Manifest Load(string dir)
    {
        var o = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        return JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Path.Combine(dir, "manifest.json")), o);
    }
}
