using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

static class VisualCppRedistributable
{
    internal const string Address = "https://aka.ms/vs/17/release/vc_redist.x64.exe";

    internal static readonly string FileName = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.exe");

    static CancellationTokenSource source = null;

    internal static void Cancel() { source?.Cancel(); while (source != null) ; }

    internal static void Install(IntPtr hwnd)
    {
        source = new();

        SHELLEXECUTEINFO pExecInfo = new()
        {
            cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(),
            hwnd = hwnd,
            fMask = 0x00000040,
            lpFile = FileName,
            lpParameters = "/repair /quiet /norestart",
            lpVerb = "runas"
        };
        NativeMethods.ShellExecuteEx(ref pExecInfo);
        NativeMethods.WaitForSingleObject(pExecInfo.hProcess, 0xFFFFFFFF);
        NativeMethods.CloseHandle(pExecInfo.hProcess);

        NativeMethods.DeleteFile(FileName);
        source.Dispose();
        source = null;
    }
}