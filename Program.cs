//TODO: add proper filtering not just wildcards

using System;
using System.IO;

internal static class Program
{
    static int Main(string[] args)
    {
        string? dir = null, folder = null, filter = null;
        bool dryRun = false;

        //CLI
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--dir": dir = NextValue(args, ref i); break;
                case "--folder": folder = NextValue(args, ref i); break;
                case "--filter": filter = NextValue(args, ref i); break;
                case "--dry-run": dryRun = true; break;
                case "-h":
                case "--help": PrintUsage(); return 0;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    PrintUsage();
                    return 1;

            }
        }

        #region validation
        if (dir is null || folder is null || filter is null)
        {
            Console.WriteLine("Missing required arguments.");
            PrintUsage();
            return 1;
        }

        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Directory not found: {dir}");
            return 1;
        }

        if (folder.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            Console.Error.WriteLine("Folder name contains invalid characters.");
            return 1;
        }
        #endregion

        string targetDir = Path.Combine(dir, folder);

        if (!dryRun)
        {
            Directory.CreateDirectory(targetDir);
        }

        int moved = 0, skipped = 0;

        foreach (string file in Directory.EnumerateFiles(dir, filter, SearchOption.TopDirectoryOnly))
        {
            string dest = Path.Combine(targetDir, Path.GetFileName(file));

            if (File.Exists(dest))
            {
                Console.WriteLine($"Skipped (already exists): {Path.GetFileName(file)}");
                skipped++;
                continue;
            }

            try
            {
                if (dryRun)
                    Console.WriteLine($"[dry run] {file} -> {dest}");
                else
                {
                    File.Move(file, dest);
                    Console.WriteLine($"Moved: {Path.GetFileName(file)}");
                }
                moved++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Failed: {Path.GetFileName(file)} ({ex.Message})");
                skipped++;
            }
        }

        Console.WriteLine($"\nDone. Moved: {moved}, skipped: {skipped}.");
        return 0;

    }

    static string NextValue(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Missing value for {args[i]}");
        return args[++i];
    }

    static void PrintUsage()
    {
        Console.WriteLine("""
        Usage: fileSorter --dir <path> --folder <name> --filter <pattern> [--dry-run]
 
          --dir      Directory to work in
          --folder   Name of the folder to create inside --dir
          --filter   File pattern to match, e.g. "*.png" or "report_*.pdf"
          --dry-run  Show what would be moved without changing anything
        """);
    }
}