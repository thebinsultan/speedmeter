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
- Single-instance named mutex (`Global\NetSpeedMonitor_SingleInstance`).
- Counter resets, adapter changes, sleep/hibernate: next poll recomputes from
  fresh counters; negative deltas clamped to 0.
- Explorer restart: `TaskbarCreated` message triggers reposition.
- All polling wrapped in try/catch — the tray loop never crashes the app.

## Security / Privacy
- Runs asInvoker (no admin), no telemetry/analytics/ads, no network calls
  except the optional update check (GitHub Releases API).
