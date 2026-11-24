# NetSpeed Monitor

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

See [Releases](../../releases) for:
- `NetSpeedMonitor-portable-win-x64.zip` / `win-arm64` — portable, no install needed.
- `NetSpeedMonitor-Setup-1.0.0.exe` — installer (optional "Start with Windows").

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

Automated xUnit tests cover speed calculation, unit conversion, adapter selection, usage aggregation, and settings persistence — run in CI on every push.

> **Note:** UI/taskbar behavior (positioning, themes, Explorer restart) requires manual testing on Windows — see the checklist in [docs/BUILD.md](docs/BUILD.md). CI validates compilation and all Core logic.

## License

MIT — see [LICENSE](LICENSE). Contributions welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).
