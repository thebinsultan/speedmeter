# NetSpeed Monitor

> **Status:** v1.0.0 released — built and tested by GitHub Actions on
> windows-latest (.NET 8). Portable x64/ARM64 and an installer are attached to the
> release; see [docs/CI_STATUS.md](docs/CI_STATUS.md) for the build/test report.

A lightweight, native Windows utility that shows **real-time download and upload speeds directly on the Windows taskbar** — no Electron, no telemetry, no admin rights, works offline.

```
↓ 2.45 MB/s
↑ 128 KB/s
```

## Features

- **Real-time speeds** from actual network interface counters (no speed-test traffic), updated every 1s by default (500ms–5s configurable).
- **Automatic units** — B/s, KB/s, MB/s, GB/s (or bits per second if you prefer).
- **Wi-Fi + Ethernet** — automatic adapter detection (Ethernet preferred) or manual selection.
- **Taskbar overlay** — compact two-line (or horizontal) display beside the system tray, theme-aware (light/dark), DPI-aware, Windows 10 & 11, repositions when the taskbar moves/resizes or Explorer restarts.
- **System tray icon** — right-click menu: Show/Hide, Network Statistics, Settings, Reset Usage Statistics, Start with Windows, Check for Updates, About, Exit. Click the overlay or tray icon to open statistics.
- **Network Statistics** — current/peak speeds, today/week/month totals, real-time graph, active adapter, local IP, connection type. History persists across restarts.
- **Settings** — refresh interval, units, adapter, layout, font, colors, position offset, transparency, theme, startup, hide-when-disconnected/zero, usage tracking. All persisted locally.
- **Lightweight** — target <30 MB idle RAM, <0.5% CPU. Single-instance, sleep/resume and Explorer-restart tolerant.

## Download

| Asset | Link |
|---|---|
| Portable (Windows x64, self-contained) | [NetSpeedMonitor-portable-win-x64.zip](https://github.com/thebinsultan/speedmeter/releases/download/v1.0.0/NetSpeedMonitor-portable-win-x64.zip) |
| Portable (Windows ARM64, self-contained) | [NetSpeedMonitor-portable-win-arm64.zip](https://github.com/thebinsultan/speedmeter/releases/download/v1.0.0/NetSpeedMonitor-portable-win-arm64.zip) |
| Installer (per-user, no admin) | [NetSpeedMonitor-Setup-1.0.0.exe](https://github.com/thebinsultan/speedmeter/releases/download/v1.0.0/NetSpeedMonitor-Setup-1.0.0.exe) |

All releases: [Releases page](https://github.com/thebinsultan/speedmeter/releases).
Unzip the portable build anywhere and run `NetSpeedMonitor.exe`; no .NET runtime
install is required. The installer adds Start-menu/desktop shortcuts and an
optional "Start with Windows" task.

## Usage

1. Run the portable `.exe` or install via the setup.
2. Speeds appear on the taskbar; the tray icon appears in the system tray.
3. Click the speeds (or tray icon) for statistics. Right-click the tray icon for settings and other options.
4. Drag the overlay to reposition it; fine-tune offset in Settings.

Data is stored only on your PC: `%LocalAppData%\NetSpeedMonitor\` (`settings.json`, `usage.json`).

## Build from source

Requires Windows + .NET 8 SDK:

```powershell
dotnet restore NetSpeedMonitor.sln
dotnet build NetSpeedMonitor.sln -c Release
dotnet test NetSpeedMonitor.sln -c Release
dotnet publish src/NetSpeedMonitor/NetSpeedMonitor.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64
```

See [docs/BUILD.md](docs/BUILD.md) and [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Project structure

```
src/NetSpeedMonitor.Core/   Pure logic (speed calc, formatting, usage, settings) — unit-tested
src/NetSpeedMonitor/        WinForms app (taskbar overlay, tray, statistics, settings)
tests/NetSpeedMonitor.Tests/ xUnit tests
installer/                  Inno Setup script
.github/workflows/          CI (build+test) and Release (tag → GitHub Release)
```

## Testing

`dotnet test` runs 45 xUnit cases covering speed calculation (counter reset,
elapsed-time handling), unit conversion (B/s → TB/s and bits/s), adapter
selection/preference, usage aggregation (peaks, week/month, 62-day prune) and
settings persistence (round-trip, normalization, corrupt-file recovery).
GitHub Actions runs them on every push and PR, and again in the Release pipeline.

> **Verified in CI:** compile (0 errors, 0 code warnings), all 45 tests, both
> self-contained publishes, installer compilation and asset upload — see
> [docs/CI_STATUS.md](docs/CI_STATUS.md).
>
> **Not verified automatically:** taskbar rendering/positioning, light/dark theme,
> DPI scaling, sleep/resume, Explorer restart and install/uninstall need a manual
> pass on Windows — checklist in [docs/BUILD.md](docs/BUILD.md).

## Known limitations

- Windows 11 centres taskbar icons; the overlay is positioned in the free space
  immediately left of the notification area, so it never covers tray icons.
  On a very crowded taskbar it can overlap app buttons — drag it, or use the
  offset settings.
- The overlay is a topmost window over the taskbar, not a hosted taskbar element
  (Win11's taskbar is XAML and cannot be extended without injection). Full-screen
  exclusive apps may draw over it; windows in "always on top" mode can cover it.
- Windows 11's "secondary taskbar" per monitor is not tracked; the overlay follows
  the primary taskbar.
- Only the explicitly selected adapter is measured in manual mode (VPN/virtual
  adapters show zero unless selected).
- Usage history keeps 62 days of daily buckets (enough for week + month views).
- ARM64 builds are produced but have not been run on ARM64 hardware here.

## License

MIT — see [LICENSE](LICENSE). Contributions welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).
