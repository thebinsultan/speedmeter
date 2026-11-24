namespace NetSpeedMonitor;

/// <summary>Compact about dialog: version, purpose, license and project link.</summary>
public sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About NetSpeed Monitor";
        ClientSize = new Size(380, 220);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        string version = Application.ProductVersion;
        if (version.Length > 0)
        {
            int plus = version.IndexOf('+');
            if (plus > 0) version = version[..plus];
        }

        var title = new Label
        {
            Text = "NetSpeed Monitor",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var body = new Label
        {
            Text = $"Version {version}\r\n\r\n" +
                   "Real-time download and upload speeds on the Windows taskbar.\r\n" +
                   "No telemetry, no ads, works offline.\r\n\r\n" +
                   "© 2026 NetSpeed Monitor Contributors — MIT License",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(16, 0, 16, 0)
        };
        var link = new LinkLabel
        {
            Text = "github.com/thebinsultan/NetSpeed-Monitor",
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter
        };
        link.LinkClicked += (_, _) => OpenProjectPage();

        var close = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.OK,
            Dock = DockStyle.Bottom,
            Height = 34
        };

        Controls.Add(body);
        Controls.Add(title);
        Controls.Add(link);
        Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;
    }

    private static void OpenProjectPage()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = UpdateChecker.ReleasesPage,
                UseShellExecute = true
            });
        }
        catch { /* no browser available */ }
    }
}
