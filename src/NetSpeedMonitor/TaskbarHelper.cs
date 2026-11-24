using System.Runtime.InteropServices;

namespace NetSpeedMonitor;

/// <summary>
/// Locates the Windows taskbar (Win10 Shell_TrayWnd / Win11) via public Win32 APIs.
/// No injection into Explorer; the overlay is a normal top-level window.
/// </summary>
public static class TaskbarHelper
{
    public enum TaskbarPosition { Bottom, Top, Left, Right, Unknown }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("shell32.dll")]
    private static extern int SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    private const uint ABM_GETTASKBARPOS = 0x00000005;
    private const uint ABM_GETSTATE = 0x00000004;
    private const uint ABE_LEFT = 0, ABE_TOP = 1, ABE_RIGHT = 2, ABE_BOTTOM = 3;

    public static bool TryGetTaskbarRect(out Rectangle rect, out TaskbarPosition position)
    {
        rect = Rectangle.Empty;
        position = TaskbarPosition.Unknown;
        try
        {
            var data = new APPBARDATA { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
            if (SHAppBarMessage(ABM_GETTASKBARPOS, ref data) != 0)
            {
                rect = Rectangle.FromLTRB(data.rc.Left, data.rc.Top, data.rc.Right, data.rc.Bottom);
                position = data.uEdge switch
                {
                    ABE_BOTTOM => TaskbarPosition.Bottom,
                    ABE_TOP => TaskbarPosition.Top,
                    ABE_LEFT => TaskbarPosition.Left,
                    ABE_RIGHT => TaskbarPosition.Right,
                    _ => TaskbarPosition.Unknown
                };
                return true;
            }
            IntPtr hwnd = FindWindow("Shell_TrayWnd", null);
            if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out var r))
            {
                rect = Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
                position = TaskbarPosition.Bottom;
                return true;
            }
        }
        catch { /* fall through */ }
        return false;
    }

    public static Rectangle PrimaryScreenBounds()
    {
        try { return Screen.PrimaryScreen.Bounds; }
        catch { return new Rectangle(0, 0, 1920, 1080); }
    }

    /// <summary>Computes overlay position next to the system tray, honoring offsets.</summary>
    public static Point ComputeOverlayPosition(Size overlaySize, int offsetX, int offsetY)
    {
        if (!TryGetTaskbarRect(out var taskbar, out var pos))
        {
            var screen = PrimaryScreenBounds();
            return new Point(screen.Right - overlaySize.Width - 200 + offsetX,
                            screen.Bottom - overlaySize.Height - 4 + offsetY);
        }

        return pos switch
        {
            TaskbarPosition.Bottom => new Point(
                taskbar.Right - overlaySize.Width - 190 + offsetX,
                taskbar.Top + (taskbar.Height - overlaySize.Height) / 2 + offsetY),
            TaskbarPosition.Top => new Point(
                taskbar.Right - overlaySize.Width - 190 + offsetX,
                taskbar.Top + (taskbar.Height - overlaySize.Height) / 2 + offsetY),
            TaskbarPosition.Left => new Point(
                taskbar.Left + (taskbar.Width - overlaySize.Width) / 2 + offsetX,
                taskbar.Bottom - overlaySize.Height - 190 + offsetY),
            TaskbarPosition.Right => new Point(
                taskbar.Left + (taskbar.Width - overlaySize.Width) / 2 + offsetX,
                taskbar.Bottom - overlaySize.Height - 190 + offsetY),
            _ => new Point(taskbar.Right - overlaySize.Width - 190 + offsetX,
                           taskbar.Top + (taskbar.Height - overlaySize.Height) / 2 + offsetY)
        };
    }
}
