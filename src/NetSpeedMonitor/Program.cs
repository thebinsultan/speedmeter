using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Single-instance guard: prevent multiple copies running at once.
        using var mutex = new Mutex(initiallyOwned: false, @"Global\NetSpeedMonitor_SingleInstance");
        bool createdNew;
        try { createdNew = mutex.WaitOne(TimeSpan.Zero, exitContext: false); }
        catch (UnauthorizedAccessException) { createdNew = false; }
        if (!createdNew)
        {
            MessageBox.Show("NetSpeed Monitor is already running.", "NetSpeed Monitor",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var settings = SettingsStore.LoadSettings();
        Application.Run(new TrayApplicationContext(settings));
        mutex.ReleaseMutex();
    }
}
