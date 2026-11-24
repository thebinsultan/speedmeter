# Build / CI Status — final

_Last updated 2026-11-24, after the v1.0.0 release._

## Result: green

| Pipeline | Run | Result |
|---|---|---|
| Build and Test (PR #1, `4a8f617`) | 45357085151 | ✅ success — restore, build, test, publish x64 + arm64, upload artifacts |
| Build and Test (`main`, merge `5de2b72`) | 45357322447 | ✅ success |
| Build and Test (`main`, `6462a3b`) | 45357650566 | ✅ success |
| Release (`v1.0.0`) | 45357925152 | ✅ success — build, test, publish, installer, GitHub Release |

Earlier failing runs, for the record:

| Run | Failure | Fix |
|---|---|---|
| 45353040882 … 45354252544 | `The job was not started because your account is locked due to a billing issue.` (account-level Actions block) | resolved by account owner |
| 45354252544 | `CS1739: The best overload for 'DeleteValue' does not have a parameter named 'throwIfMissing'` + xUnit2013 + WFAC010 warnings | `throwOnMissingValue`, `Assert.Single`, DPI moved from `app.manifest` to `<ApplicationHighDpiMode>PerMonitorV2</...>` |
| 45357335713 | Inno Setup `ISCC` exit code 1 (output not captured by the hosted runner) | multi-size `icon.ico` (256/128/64/48/32/24/16), removed 3-part `VersionInfoVersion`, ISCC output now mirrored into `::error` annotations |

## Build output

- Build configuration: `Release`, `windows-latest`, .NET SDK 8.x.
- Compile errors: **0**. Compiler/analyzer code warnings: **0** (the only remaining
  annotation is GitHub's `Node.js 20 is deprecated` notice for the action versions).
- Tests: `dotnet test --collect:"XPlat Code Coverage"` — **45 test cases** across
  `SpeedCalculatorTests` (8), `UnitFormatterTests` (16), `AdapterSelectorTests` (7),
  `UsageTrackerTests` (5), `SettingsStoreTests` (9). All pass.
- Publishing: `PublishSingleFile` self-contained `win-x64` and `win-arm64`.

## Released assets (v1.0.0)

| Asset | Size |
|---|---|
| `NetSpeedMonitor-portable-win-x64.zip` | 62,602,871 bytes |
| `NetSpeedMonitor-portable-win-arm64.zip` | 57,977,862 bytes |
| `NetSpeedMonitor-Setup-1.0.0.exe` | 47,493,037 bytes |

## Still requires manual verification on Windows

Taskbar placement/visuals on Win10 and Win11 (bottom/top/left/right, DPI 150 %,
light/dark), sleep/resume, Explorer restart, install/uninstall, and the
< 30 MB RAM / < 0.5 % CPU targets. Checklist: [BUILD.md](BUILD.md).

## Repository naming

The connected GitHub token can commit but cannot create repositories
(`POST /user/repos` → `403`), so the project lives in
`thebinsultan/speedmeter`. Renaming that repository to `NetSpeed-Monitor` in
*Settings → General* requires no code changes: the workflows, installer paths and
the update checker (`api.github.com/repos/thebinsultan/NetSpeed-Monitor/releases/latest`)
already use the final name.
