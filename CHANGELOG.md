# Changelog

All notable changes to NetSpeed Monitor will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-11-24

### Added
- Real-time download/upload speeds on the Windows taskbar (two-line overlay).
- Automatic unit conversion (B/s, KB/s, MB/s, GB/s) and optional bits-per-second display.
- Automatic network adapter detection (Ethernet preferred, then Wi-Fi) and manual selection.
- System tray icon with full context menu (Show/Hide, Statistics, Settings, Reset, Start with Windows, Check for Updates, About, Exit).
- Network Statistics window: current/peak speeds, today/week/month totals, real-time graph, adapter, local IP, connection type.
- Persistent local usage history (`%LocalAppData%\NetSpeedMonitor\usage.json`).
- Settings window: refresh interval (500ms–5s), layout, font, colors, offsets, transparency, theme, startup, visibility rules, usage tracking.
- Light/dark/system theme support, DPI-aware rendering, Windows 10/11 taskbar positioning.
- Single-instance guard, Explorer-restart recovery, sleep/resume tolerance.
- Portable self-contained `.exe` (win-x64, win-arm64) and Inno Setup installer.
- GitHub Actions CI (build + test) and Release workflow.

### Changed
- Initial stable release.

### Fixed
- Counter-reset handling (adapter restart no longer yields negative speeds).
- Corrupt settings/usage files fall back to defaults instead of crashing.
