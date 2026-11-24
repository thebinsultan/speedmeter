# Architecture

## Overview
NetSpeed Monitor is a native C# (.NET 8) WinForms application. No Electron, no
embedded browser, no external services. It reads real interface counters and
renders speeds on a taskbar-aligned overlay.

## Why not inject into the taskbar?
- Windows 10 taskbar is hosted by Explorer (Shell_TrayWnd); Windows 11's is
  XAML-based and cannot be safely injected into.
- Injection / undocumented hooks are fragile, break on updates, and violate
  the "do not modify system files / inject into Explorer" requirement.
- Instead we use a **borderless, topmost, no-activate overlay window**
  positioned over the taskbar via public Win32 APIs (`SHAppBarMessage`
  `ABM_GETTASKBARPOS`, `FindWindow("Shell_TrayWnd")`, `GetWindowRect`).
  It visually matches the taskbar (theme-aware background/foreground),
  is DPI-aware (per-monitor v2 manifest), and repositions on
  `WM_DISPLAYCHANGE`, `WM_DPICHANGED`, and the `TaskbarCreated` message
  (Explorer restart).

## Components
| Component | Responsibility |
|---|---|
| `NetSpeedMonitor.Core` | Pure logic: `SpeedCalculator`, `UnitFormatter`, `UsageTracker`, `AdapterSelector`, `SettingsStore`, `AppSettings`. No WinForms, fully unit-tested. |
| `NetworkMonitor` | Enumerates adapters via `System.Net.NetworkInformation`, maps to Core DTOs, computes speeds. |
| `TaskbarOverlayForm` | Two-line (or horizontal) speed display, drag-to-move, click-to-open-stats. |
| `TrayApplicationContext` | Tray icon + menu, polling timer, usage persistence, single-instance mutex. |
| `StatisticsForm` + `SpeedGraphControl` | Stats window with GDI+ real-time graph. |
| `SettingsForm` | All preferences, persisted to JSON. |
| `ThemeHelper` | Light/dark detection via `AppsUseLightTheme` registry value. |
| `StartupHelper` | HKCU `Run` key for "Start with Windows" (no admin). |
| `UpdateChecker` | GitHub Releases API check, offline-tolerant. |

## Data Flow
```
NetworkInterface.GetIPStatistics()  (BytesReceived / BytesSent, cumulative)
        ↓  every RefreshIntervalMs (default 1s)
SpeedCalculator.Update()  →  bytes/sec (handles counter reset)
        ↓
UnitFormatter  →  "2.45 MB/s" / "128 KB/s" (1024-based, auto unit)
        ↓
TaskbarOverlayForm (paint)  +  UsageTracker.Record(delta)  →  usage.json (every 10 ticks)
```

## Performance
- One WinForms timer at the configured interval; no busy loops, no extra processes.
- Single-file self-contained publish; idle target <30 MB RAM, <0.5% CPU.
- Speeds come from OS counters — zero synthetic traffic, works offline.

## Persistence
- `%LocalAppData%\NetSpeedMonitor\settings.json`
- `%LocalAppData%\NetSpeedMonitor\usage.json` (daily buckets, last 62 days, peaks)
- Corrupt/missing files fall back to defaults.

## Resilience
- Single-instance named mutex (`Global\NetSpeedMonitor_SingleInstance`), with
  `AbandonedMutexException` handled so a crashed previous instance cannot block start-up.
- Counter resets / adapter restarts: negative deltas are clamped to 0, so the
  overlay shows 0 B/s for one interval instead of a nonsense spike.
- Sleep / hibernation: if more than 10 s elapse between polls the calculator is
  re-based (`SpeedCalculator.Reset()`), preventing a bogus "average since resume"
  figure and excluding the slept interval from usage totals.
- Explorer restart: `TaskbarCreated` (registered window message) plus
  `WM_DISPLAYCHANGE` / `WM_DPICHANGED` / `WM_SETTINGCHANGE` trigger a re-measure
  and re-position, so the overlay survives a taskbar rebuild, resolution change,
  DPI change or theme switch.
- Taskbar geometry uses `SHAppBarMessage(ABM_GETTASKBARPOS)` with a
  `Shell_TrayWnd` fallback, and `TrayNotifyWnd` to find the free space before the
  notification area — the overlay never covers clock or tray icons.
- The overlay is sized from a worst-case template (`888.8 MB/s`) once, so updates
  never resize or jitter it (no per-tick layout work).
- All polling is wrapped in try/catch — a transient adapter or permission error
  can never crash the tray loop.
- Unhandled start-up errors are appended to `%LocalAppData%\NetSpeedMonitor\error.log`
  and surfaced in a dialog rather than failing silently.

## Security / Privacy
- Runs asInvoker (no admin), no telemetry/analytics/ads, no network calls
  except the optional update check (GitHub Releases API).
