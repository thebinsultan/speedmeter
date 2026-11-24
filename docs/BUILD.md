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
   Output: `installer\Output\NetSpeedMonitor-Setup-1.0.0.exe`

## CI / Releases
- `.github/workflows/build.yml` — on every push/PR: restore, build, test, publish x64+arm64, upload artifacts.
- `.github/workflows/release.yml` — on `v*` tags: build, test, publish, build installer, publish GitHub Release with portable ZIPs + installer.

## Verification without Windows (what has/has not been run)

| Check | How | Result |
|---|---|---|
| C# syntax of all 24 source/test files | tree-sitter C# grammar parse (0 error nodes) | pass |
| XML validity (csproj, props, manifest) | `xml.etree` parse | pass |
| Unit-test algorithm correctness | algorithms ported 1:1 to Python, all assertions re-checked | pass (see below) |
| `dotnet build` / `dotnet test` / `dotnet publish` | GitHub Actions, windows-latest | **pass** (run 45357650566) |
| Taskbar rendering, themes, DPI, sleep/resume, installer | manual, requires Windows | **not run** |

The Core logic is deliberately platform-independent so that it *can* be tested
anywhere. Every `UnitFormatter`, `SpeedCalculator` and `UsageTracker` expectation
in `tests/` was reproduced with the identical algorithm and passed, e.g.:

```
0 B/s          -> "0.00 B/s"        1024 B/s      -> "1.00 KB/s"
128 KB/s       -> "128 KB/s"        2.45 MB/s     -> "2.45 MB/s"
1 GiB/s        -> "1.00 GB/s"       1 GiB/s (bits) -> "8.00 Gbps"
counter reset  -> 0 B/s (never negative)
elapsed 2 s / 2000 B -> 1000 B/s (not assumed 1 s)
```

## CI results for v1.0.0

- Build and Test: run 45357085151 (PR #1) and 45357650566 (`main`) — success.
- Release: run 45357925152 (tag `v1.0.0`) — success, 3 assets published.
- Tests: 45 xUnit cases, all passing; 0 compiler errors and 0 code warnings.

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
