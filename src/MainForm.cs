using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

class MainForm : Form
{
    enum Units { B, KB, MB, GB }

    static string Format(float bytes)
    {
        int value = 0;
        while (bytes >= 1024f) { bytes /= 1024f; value++; }
        return string.Format($"{bytes:0.00} {(Units)value}");
    }

    internal MainForm()
    {
        Font = new("MS Shell Dlg 2", 8);
        Text = "Smoothie Installer";
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = LogicalToDeviceUnits(new System.Drawing.Size(380, 115));
        CenterToScreen();

        Label label1 = new()
        {
            Text = "Updating Smoothie...",
            Width = LogicalToDeviceUnits(359),
            Height = LogicalToDeviceUnits(13),
            Location = new(LogicalToDeviceUnits(9), LogicalToDeviceUnits(23)),
            Margin = default
        };
        Controls.Add(label1);

        ProgressBar progressBar = new()
        {
            Width = LogicalToDeviceUnits(359),
            Height = LogicalToDeviceUnits(23),
            Location = new(LogicalToDeviceUnits(11), LogicalToDeviceUnits(46)),
            Margin = default,
            MarqueeAnimationSpeed = 30,
            Style = ProgressBarStyle.Marquee
        };
        Controls.Add(progressBar);

        Label label2 = new()
        {
            Text = "Checking...",
            Width = LogicalToDeviceUnits(275),
            Height = LogicalToDeviceUnits(13),
            Location = new(label1.Location.X, LogicalToDeviceUnits(80)),
            Margin = default
        };
        Controls.Add(label2);

        Button button = new()
        {
            Text = "Cancel",
            Width = LogicalToDeviceUnits(75),
            Height = LogicalToDeviceUnits(23),
            Location = new(LogicalToDeviceUnits(294), LogicalToDeviceUnits(81)),
            Margin = default
        };
        button.Click += (sender, e) => Close();
        Controls.Add(button);

        using WebClient client = new();
        string value = default, fileName = default;

        client.DownloadProgressChanged += (sender, e) =>
        {
            var text = $"Downloading {Format(e.BytesReceived)} / {value ??= Format(e.TotalBytesToReceive)}";
            Invoke(() =>
            {
                label2.Text = text;
                if (progressBar.Value != e.ProgressPercentage) progressBar.Value = e.ProgressPercentage;
            });
        };
        client.DownloadFileCompleted += (sender, e) => value = null;


        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            client.CancelAsync();
            while (client.IsBusy) ;
            NativeMethods.DeleteFile(fileName);
            Installer.Cancel();
            FFmpeg.Cancel();
            VisualCppRedistributable.Cancel();
        };

        Shown += async (sender, e) => await Task.Run(() =>
        {
            Installer.CreateEntry();
            var address = Installer.Start();

            Invoke(() =>
            {
                label2.Text = "Downloading...";
                progressBar.Style = ProgressBarStyle.Blocks;
            });
            fileName = Path.GetTempFileName();
            client.DownloadFileTaskAsync(address, fileName).Wait();

            Invoke(() =>
            {
                label2.Text = "Extracting...";
                progressBar.Style = ProgressBarStyle.Marquee;
            });
            Installer.Extract(fileName);

            if (!FFmpeg.Check())
            {
                Invoke(() =>
                {
                    label1.Text = "Updating FFmpeg...";
                    label2.Text = "Downloading...";
                    progressBar.Value = 0;
                    progressBar.Style = ProgressBarStyle.Blocks;
                });
                fileName = Path.GetTempFileName();
                client.DownloadFileTaskAsync(FFmpeg.Address, fileName).Wait();

                Invoke(() =>
                {
                    label2.Text = "Extracting...";
                    progressBar.Style = ProgressBarStyle.Marquee;
                });
                FFmpeg.Extract(fileName);
            }

            Invoke(() =>
            {
                label1.Text = "Updating Visual C++ Redistributable...";
                label2.Text = "Downloading...";
                progressBar.Value = 0;
                progressBar.Style = ProgressBarStyle.Blocks;
            });
            client.DownloadFileTaskAsync(VisualCppRedistributable.Address, VisualCppRedistributable.FileName).Wait();

            Invoke(() =>
            {
                label2.Text = "Waiting...";
                progressBar.Style = ProgressBarStyle.Marquee;
            });
            VisualCppRedistributable.Install(Handle);

            Close();
        });
    }
}