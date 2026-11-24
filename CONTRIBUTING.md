# Contributing to NetSpeed Monitor

Thanks for your interest! This is a small, focused native Windows utility.

## Prerequisites
- Windows 10/11 (x64 or ARM64)
- .NET 8 SDK
- Git

## Build & Test
```powershell
dotnet restore NetSpeedMonitor.sln
dotnet build NetSpeedMonitor.sln -c Release
dotnet test NetSpeedMonitor.sln -c Release
dotnet publish src/NetSpeedMonitor/NetSpeedMonitor.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64
```

## Project Layout
- `src/NetSpeedMonitor.Core/` — platform-independent logic (speed calc, formatting, usage, settings). Fully unit-tested.
- `src/NetSpeedMonitor/` — WinForms app (taskbar overlay, tray, windows).
- `tests/NetSpeedMonitor.Tests/` — xUnit tests for Core.
- `installer/` — Inno Setup script.
- `docs/` — architecture and build docs.

## Guidelines
- Keep dependencies minimal; no Electron, no telemetry, no admin required.
- Core logic must stay UI-free so it remains testable on any OS.
- Run `dotnet test` before submitting; all tests must pass.
- Follow existing code style (nullable enabled, implicit usings, LangVersion 12).
- Update `CHANGELOG.md` for user-visible changes.

## Pull Requests
1. Fork and create a feature branch.
2. Make your change with tests where applicable.
3. Ensure CI (GitHub Actions) is green.
4. Open a PR with a clear description.

## Reporting Issues
Include Windows version, adapter type (Wi-Fi/Ethernet), and steps to reproduce.
