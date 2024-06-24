using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Collections.Generic;

static class FFmpeg
{
    internal const string Address = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl-shared.zip";

    static readonly IEnumerable<string> paths = ["ffmpeg.exe", "ffprobe.exe", "ffplay.exe"];

    static CancellationTokenSource source = null;

    internal static bool Check()
    {
        foreach (var path in paths) if (!NativeMethods.PathFindOnPath(new(path, 260))) return false;
        return true;
    }

    internal static void Cancel() { source?.Cancel(); while (source != null) ; }

    internal static void Extract(string archiveFileName)
    {
        source = new();

        using ZipArchive zip = ZipFile.OpenRead(archiveFileName);
        foreach (var entry in zip.Entries)
        {
            if (source.IsCancellationRequested) return;
            if (entry.Name.EndsWith(".exe") || entry.Name.EndsWith(".dll") || entry.Name.Equals("LICENSE.txt"))
                entry.ExtractToFile(Path.Combine(Installer.BinariesPath, entry.Name), true);
        }

        NativeMethods.DeleteFile(archiveFileName);
        source.Dispose();
        source = null;
    }
}