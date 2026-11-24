namespace NetSpeedMonitor;

public sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About NetSpeed Monitor";
        Width = 380;
        Height = 220;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        var label = new Label
        {
            Dock = DockStyle.Fill,
            Text = "NetSpeed Monitor v1.0.0\r\n\r\nLightweight real-time network speed monitor\r\nfor the Windows taskbar.\r\n\r\n© 2026 NetSpeed Monitor Contributors\r\nMIT License",
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(16)
        };
        Controls.Add(label);
    }
}
