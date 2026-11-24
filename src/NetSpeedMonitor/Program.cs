using NetSpeedMonitor.Core;

namespace NetSpeedMonitor;

internal static class Program
{
    private const string MutexName = @"Global\NetSpeedMonitor_SingleInstance";

    [STAThread]
    private static void Main()
    {
        // Per-monitor v2 DPI must be set before any window is created (also declared
        // in app.manifest for the primary monitor case).
        try { Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); } catch { /* ignore */ }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Single-instance guard: prevent multiple copies running at once.
        using var mutex = new Mutex(initiallyOwned: false, MutexName);
        bool owned;
        try { owned = mutex.WaitOne(TimeSpan.Zero, exitContext: false); }
        catch (AbandonedMutexException) { owned = true; }
        catch (UnauthorizedAccessException) { owned = false; }

        if (!owned)
        {
            MessageBox.Show("NetSpeed Monitor is already running. Look for the tray icon.",
                "NetSpeed Monitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var settings = SettingsStore.LoadSettings();
            Application.Run(new TrayApplicationContext(settings));
        }
        catch (Exception ex)
        {
            try
            {
                var log = Path.Combine(SettingsStore.DataDirectory, "error.log");
                Directory.CreateDirectory(SettingsStore.DataDirectory);
                File.AppendAllText(log, $"[{DateTimeOffset.Now:O}] {ex}{Environment.NewLine}");
            }
            catch { /* logging is best effort */ }
            MessageBox.Show("NetSpeed Monitor encountered an error and must close:\n" + ex.Message,
                "NetSpeed Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            try { mutex.ReleaseMutex(); } catch { /* not owned */ }
        }
    }
}
