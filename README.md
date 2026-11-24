# NetSpeed Monitor

> **Release status:** source complete and pushed; **v1.0.0 is not released yet**.
> GitHub Actions is currently blocked on this account ("account locked due to a
> billing issue"), so the build/test/release pipeline has not produced binaries.
> See [docs/CI_STATUS.md](docs/CI_STATUS.md). No binaries are claimed until CI passes.

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

Releases are published from tags by the `Release` workflow. Until the first
green build exists, build from source (below) — see [docs/CI_STATUS.md](docs/CI_STATUS.md).

Planned assets per release:
- `NetSpeedMonitor-portable-win-x64.zip` / `-win-arm64.zip` — portable, no install needed.
- `NetSpeedMonitor-Setup-<version>.exe` — installer (optional "Start with Windows" task).

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

Automated xUnit tests cover speed calculation (including counter reset and
elapsed-time handling), unit conversion, adapter selection, usage aggregation and
settings persistence. They run via `dotnet test` and are wired into the
**Build and Test** workflow (`dotnet test` + coverage collector) on every push and PR.

> **Status:** those workflow runs are currently blocked before the first step by
> an account-level Actions lock (see [docs/CI_STATUS.md](docs/CI_STATUS.md)), so no
> CI test result exists yet. Syntax of every file, project XML validity and the
> Core algorithms have been verified independently — see
> [docs/BUILD.md](docs/BUILD.md#verification-without-windows-what-has-has-not-been-run).

UI/taskbar behaviour (positioning, themes, DPI, Explorer restart, installer) needs
manual testing on Windows — checklist in [docs/BUILD.md](docs/BUILD.md).

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
