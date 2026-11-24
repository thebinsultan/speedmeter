# Build & Distribution

## Prerequisites
- Windows 10/11 x64 or ARM64
- .NET 8 SDK (`dotnet --version` → 8.x)

## Build
```powershell
dotnet restore NetSpeedMonitor.sln
dotnet build NetSpeedMonitor.sln -c Release
dotnet test NetSpeedMonitor.sln -c Release
```

## Portable executable (self-contained, no runtime install needed)
```powershell
# x64
dotnet publish src/NetSpeedMonitor/NetSpeedMonitor.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64 /p:PublishSingleFile=true
# ARM64
dotnet publish src/NetSpeedMonitor/NetSpeedMonitor.csproj -c Release -r win-arm64 --self-contained true -o publish/win-arm64 /p:PublishSingleFile=true
```
Run `publish\win-x64\NetSpeedMonitor.exe` directly — no installation required.

## Installer (Inno Setup)
1. Install Inno Setup 6.
2. Publish as above.
3. Compile `installer\NetSpeedMonitor.iss`:
   ```powershell
   & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\NetSpeedMonitor.iss
   ```
   Output: `Output\NetSpeedMonitor-Setup-1.0.0.exe`

## CI / Releases
- `.github/workflows/build.yml` — on every push/PR: restore, build, test, publish x64+arm64, upload artifacts.
- `.github/workflows/release.yml` — on `v*` tags: build, test, publish, build installer, publish GitHub Release with portable ZIPs + installer.

## Manual testing checklist (requires Windows)
- [ ] Speeds match Task Manager / `Get-NetAdapterStatistics` roughly.
- [ ] Unit switching B/s → KB/s → MB/s → GB/s.
- [ ] Adapter auto-switch (disable Wi-Fi, enable Ethernet) and manual selection.
- [ ] Taskbar position on bottom/top/left/right, resize, DPI 100%/150%.
- [ ] Light/dark theme follows Windows setting.
- [ ] Settings persist after restart; usage totals accumulate.
- [ ] Start with Windows (HKCU Run key), Start minimized.
- [ ] Sleep/resume and Explorer restart (`taskkill /f /im explorer.exe`) recover.
- [ ] Installer installs/uninstalls cleanly; portable runs without install.
- [ ] Idle RAM/CPU in Task Manager (target <30 MB, <0.5% CPU).
