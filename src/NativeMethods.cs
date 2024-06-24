using System;
using System.Text;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
struct SHELLEXECUTEINFO
{
    internal int cbSize;

    internal uint fMask;

    internal IntPtr hwnd;

    internal string lpVerb;

    internal string lpFile;

    internal string lpParameters;

    internal string lpDirectory;

    internal int nShow;

    internal IntPtr hInstApp;

    internal IntPtr lpIDList;

    internal string lpClass;

    internal IntPtr hkeyClass;

    internal uint dwHotKey;

    internal IntPtr hIcon;

    internal IntPtr hProcess;
}

static class NativeMethods
{
    [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern bool DeleteFile(string lpFileName);

    [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists = false);

    [DllImport("Shlwapi", CharSet = CharSet.Auto)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern bool PathFindOnPath([In, Out] StringBuilder pszPath, [In] string[] ppszOtherDirs = null);

    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern int ShellMessageBox(IntPtr hAppInst = default, IntPtr hWnd = default, string lpcText = default, string lpcTitle = "Smoothie Installer", int fuStyle = 0x00000004 | 0x00000040);

    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO pExecInfo);

    [DllImport("Kernel32")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern int WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("Kernel32")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern bool CloseHandle(IntPtr hObject);
}