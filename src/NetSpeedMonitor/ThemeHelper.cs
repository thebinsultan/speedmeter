using Microsoft.Win32;

namespace NetSpeedMonitor;

/// <summary>Detects Windows light/dark mode and resolves effective colors.</summary>
public static class ThemeHelper
{
    public static bool IsDarkMode(string themeSetting)
    {
        if (themeSetting == "Dark") return true;
        if (themeSetting == "Light") return false;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i == 0;
        }
        catch { return false; }
    }

    public static Color Foreground(string themeSetting) =>
        IsDarkMode(themeSetting) ? Color.WhiteSmoke : Color.FromArgb(0x1A, 0x1A, 0x1A);

    public static Color Background(string themeSetting) =>
        IsDarkMode(themeSetting) ? Color.FromArgb(0x20, 0x20, 0x20) : Color.FromArgb(0xF3, 0xF3, 0xF3);

    public static Color ParseColor(string hex, Color fallback)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            return ColorTranslator.FromHtml(hex);
        }
        catch { return fallback; }
    }
}
