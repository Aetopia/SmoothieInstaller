using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Xml;
using Microsoft.Win32;
using System.Threading;
using System.Reflection;
using System.IO.Compression;
using System.Management.Automation;
using System.Runtime.Serialization.Json;

static class Installer
{
    static readonly WebClient client = new();

    static CancellationTokenSource source = null;

    internal readonly static string InstallationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Smoothie");

    internal readonly static string BinariesPath = Path.Combine(InstallationPath, "bin");

    internal static bool Check()
    {
        StringBuilder path = new("smoothie-rs.exe", 260);
        return NativeMethods.PathFindOnPath(path) && Path.GetDirectoryName(path.ToString()).Equals(BinariesPath, StringComparison.OrdinalIgnoreCase);
    }

    internal static void Cancel() { source?.Cancel(); while (source != null) ; }

    internal static string Start()
    {
        Directory.CreateDirectory(InstallationPath);
        var value = Environment.GetEnvironmentVariable("Path");
        if (!value.Contains(BinariesPath))
            Environment.SetEnvironmentVariable("Path", $"{BinariesPath};{value.Trim(';')}", EnvironmentVariableTarget.User);

        client.Headers["User-Agent"] = "Smoothie Installer";
        using var reader = JsonReaderWriterFactory.CreateJsonReader(
            Encoding.UTF8.GetBytes(client.DownloadString("https://api.github.com/repos/couleur-tweak-tips/smoothie-rs/releases/latest")),
            XmlDictionaryReaderQuotas.Max);
        XmlDocument xml = new();
        xml.Load(reader);

        return xml.GetElementsByTagName("browser_download_url")[0].InnerText;
    }

    internal static void Extract(string archiveFileName)
    {
        source = new();
        
        using ZipArchive zip = ZipFile.OpenRead(archiveFileName);
        foreach (var entry in zip.Entries)
        {
            if (source.IsCancellationRequested) return;
            if (entry.FullName.Equals("smoothie-rs/") || entry.Name.Equals("makeShortcuts.cmd")) continue;
            string path = Path.Combine(InstallationPath, entry.FullName.Split(new string[] { "smoothie-rs/" }, StringSplitOptions.RemoveEmptyEntries)[0]);

            if (entry.Name.Equals("recipe.ini") && File.Exists(path)) continue;
            if (entry.FullName.Last().Equals(Path.AltDirectorySeparatorChar))
                Directory.CreateDirectory(path);
            else entry.ExtractToFile(path, true);
        }

        NativeMethods.DeleteFile(archiveFileName);
        source.Dispose();
        source = null;
    }

    internal static void CreateEntry()
    {
        var path = Path.Combine(BinariesPath, "SmoothieInstaller.exe");
        try { Directory.CreateDirectory(BinariesPath); }
        catch (IOException) { };
        NativeMethods.CopyFile(Assembly.GetExecutingAssembly().Location, path);

        PowerShell.Create().AddScript($$"""
$TargetPath = "{InstallationPath}"

$Path = [System.Environment]::GetFolderPath("Programs")
New-Item -ItemType "Directory" -Path $Path

$Path = "$Path\Smoothie"
New-Item -ItemType "Directory" -Path $Path

$WshShell = New-Object -ComObject "WScript.Shell"

$Shortcut = $WshShell.CreateShortcut("$Path\Smoothie.lnk")
$Shortcut.TargetPath = "$TargetPath\bin\smoothie-rs.exe"
$Shortcut.IconLocation = "$TargetPath\bin\smoothie-rs.exe"
$Shortcut.WindowStyle = 7
$Shortcut.Description = "Smoothen up your gameplay footage with Smoothie, yum!"
$Shortcut.Save()

$Shortcut = $WshShell.CreateShortcut("$Path\Smoothie Recipe.lnk")
$Shortcut.TargetPath = "$TargetPath\recipe.ini"
$Shortcut.Save()

$oldShortcut = "$([System.Environment]::GetFolderPath("SendTo"))\Smoothie.lnk"
if (Test-Path $oldShortcut){
    Remove-Item $oldShortcut
}


$Shortcut = $WshShell.CreateShortcut("$([System.Environment]::GetFolderPath("SendTo"))\&Smoothie.lnk")
$Shortcut.TargetPath = "$TargetPath\bin\smoothie-rs.exe"
$Shortcut.Arguments = "--tui -i";
$Shortcut.Save()
""".Replace("{InstallationPath}", InstallationPath)).Invoke();

        Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Smoothie", false);
        using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Smoothie", true);
        registryKey.SetValue("DisplayIcon", Path.Combine(BinariesPath, "smoothie-rs.exe"));
        registryKey.SetValue("DisplayName", "Smoothie");
        registryKey.SetValue("Publisher", "Couleur Tweak Tips");
        registryKey.SetValue("ModifyPath", path);
        var temp = Path.GetTempPath();
        registryKey.SetValue("UninstallString", @$"cmd.exe /c ""copy /Y ""{path}"" ""{temp}\SmoothieInstaller.exe"" && start """" ""{temp}\SmoothieInstaller.exe"" /Uninstall""");
    }

    internal static void Uninstall()
    {
        try { Directory.Delete(InstallationPath, true); }
        catch (DirectoryNotFoundException) { }

        try { Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Smoothie"), true); }
        catch (DirectoryNotFoundException) { }

        NativeMethods.DeleteFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), "Smoothie.lnk"));
        Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Smoothie", false);

        var value = Environment.GetEnvironmentVariable("Path").Replace(BinariesPath, string.Empty).Trim(';');
        Environment.SetEnvironmentVariable("Path", value, EnvironmentVariableTarget.User);
    }
}