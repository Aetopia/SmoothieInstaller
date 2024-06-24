using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

static class Program
{
    static void Main(string[] args)
    {
        Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User));
        using System.Threading.Mutex mutex = new(true, "D1E9A069-FDAB-4882-A754-AEB6F879E930", out bool createdNew);
        if (!createdNew) return;

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = (Exception)e.ExceptionObject;
            Installer.CreateEntry();
            while (exception.InnerException != null) exception = exception.InnerException;
            NativeMethods.ShellMessageBox(lpcText: exception.Message, fuStyle: 0x00000010);
            Environment.Exit(0);
        };

        ((NameValueCollection)ConfigurationManager.GetSection("System.Windows.Forms.ApplicationConfigurationSection"))["DpiAwareness"] = "PerMonitorV2";
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

        if (string.Equals(args.FirstOrDefault(), "/uninstall", StringComparison.OrdinalIgnoreCase))
        { if (NativeMethods.ShellMessageBox(lpcText: "Would you like to uninstall Smoothie?") == 6) Installer.Uninstall(); }
        else
        {
            if (Installer.Check() && NativeMethods.ShellMessageBox(lpcText: "Would you like to reinstall Smoothie?") != 6) return;
            new MainForm().ShowDialog();
        }
    }
}
