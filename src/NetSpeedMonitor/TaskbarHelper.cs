using System.Runtime.InteropServices;

namespace NetSpeedMonitor;

/// <summary>
/// Locates the Windows taskbar and notification area using public Win32 APIs only.
/// No injection into Explorer; the overlay is a normal top-level window.
/// </summary>
public static class TaskbarHelper
{
    public enum TaskbarPosition { Bottom, Top, Left, Right, Unknown }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter,
        string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("shell32.dll")]
    private static extern UIntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
        public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
    }

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
    private const uint ABE_LEFT = 0, ABE_TOP = 1, ABE_RIGHT = 2, ABE_BOTTOM = 3;

    /// <summary>Gets the taskbar rectangle and its screen edge.</summary>
    public static bool TryGetTaskbar(out Rectangle rect, out TaskbarPosition position)
    {
        rect = Rectangle.Empty;
        position = TaskbarPosition.Unknown;
        try
        {
            var data = new APPBARDATA { cbSize = (uint)Marshal.SizeOf<APPBARDATA>() };
            if (SHAppBarMessage(ABM_GETTASKBARPOS, ref data) != UIntPtr.Zero)
            {
                rect = data.rc.ToRectangle();
                position = data.uEdge switch
                {
                    ABE_BOTTOM => TaskbarPosition.Bottom,
                    ABE_TOP => TaskbarPosition.Top,
                    ABE_LEFT => TaskbarPosition.Left,
                    ABE_RIGHT => TaskbarPosition.Right,
                    _ => TaskbarPosition.Unknown
                };
                if (rect.Width > 0 && rect.Height > 0) return true;
            }
        }
        catch { /* fall back below */ }

        try
        {
            IntPtr hwnd = FindWindow("Shell_TrayWnd", null);
            if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out var r))
            {
                rect = r.ToRectangle();
                if (rect.Width > 0 && rect.Height > 0)
                {
                    var screen = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
                    position = rect.Top <= screen.Top + 2 ? TaskbarPosition.Top
                        : rect.Left <= screen.Left + 2 ? TaskbarPosition.Left
                        : rect.Right >= screen.Right - 2 ? TaskbarPosition.Right
                        : TaskbarPosition.Bottom;
                    return true;
                }
            }
        }
        catch { /* unavailable */ }
        return false;
    }

    /// <summary>Rectangle of the notification area (clock/tray), which marks the safe right edge.</summary>
    public static bool TryGetTrayArea(out Rectangle rect)
    {
        rect = Rectangle.Empty;
        try
        {
            IntPtr taskbar = FindWindow("Shell_TrayWnd", null);
            if (taskbar == IntPtr.Zero) return false;
            IntPtr tray = FindWindowEx(taskbar, IntPtr.Zero, "TrayNotifyWnd", null);
            if (tray == IntPtr.Zero) return false;
            if (!GetWindowRect(tray, out var r)) return false;
            rect = r.ToRectangle();
            return rect.Width > 0 && rect.Height > 0;
        }
        catch { return false; }
    }

    /// <summary>
    /// Places the overlay in the free taskbar space immediately before the notification
    /// area (where native speed meters sit), vertically centred, honouring user offsets.
    /// </summary>
    public static Point ComputeOverlayPosition(Size overlaySize, int offsetX, int offsetY)
    {
        if (!TryGetTaskbar(out var taskbar, out var position))
        {
            var screen = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            return new Point(screen.Right - overlaySize.Width - 220 + offsetX,
                             screen.Bottom - overlaySize.Height - 6 + offsetY);
        }

        bool haveTray = TryGetTrayArea(out var tray);

        switch (position)
        {
            case TaskbarPosition.Bottom:
            {
                int right = haveTray ? tray.Left : taskbar.Right - 220;
                return new Point(right - overlaySize.Width - 6 + offsetX,
                                 taskbar.Top + (taskbar.Height - overlaySize.Height) / 2 + offsetY);
            }
            case TaskbarPosition.Top:
            {
                int right = haveTray ? tray.Left : taskbar.Right - 220;
                return new Point(right - overlaySize.Width - 6 + offsetX,
                                 taskbar.Top + (taskbar.Height - overlaySize.Height) / 2 + offsetY);
            }
            case TaskbarPosition.Left:
            {
                int bottom = haveTray ? tray.Top : taskbar.Bottom - 220;
                return new Point(taskbar.Left + (taskbar.Width - overlaySize.Width) / 2 + offsetX,
                                 bottom - overlaySize.Height - 6 + offsetY);
            }
            case TaskbarPosition.Right:
            {
                int bottom = haveTray ? tray.Top : taskbar.Bottom - 220;
                return new Point(taskbar.Left + (taskbar.Width - overlaySize.Width) / 2 + offsetX,
                                 bottom - overlaySize.Height - 6 + offsetY);
            }
            default:
                return new Point(taskbar.Right - overlaySize.Width - 220 + offsetX,
                                 taskbar.Top + (taskbar.Height - overlaySize.Height) / 2 + offsetY);
        }
    }
}
