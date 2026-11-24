# CI / Build Status (2026-11-24)

## What was attempted
- Pushed the complete NetSpeed Monitor source to `thebinsultan/speedmeter`
  (PR #1, branch `arena/8388bc39-speedmeter`).
- GitHub Actions "Build and Test" triggered on the PR:
  run 45353040882 — **job not started: account locked due to a billing issue**
  (GitHub annotation on the run). This is an account-level Actions block,
  not a code failure.
- Local sandbox has no .NET SDK (Linux, `dotnet` not installed, SDK download
  hosts not reachable), so a local Windows compilation was not possible.

## What was validated
- Core algorithms (speed calculation, unit formatting B/s→GB/s and bps,
  usage week/month aggregation, adapter selection) were ported to Python
  and all unit-test cases from `tests/NetSpeedMonitor.Tests/` pass.
- Source review for compile correctness (WinForms APIs, P/Invoke signatures,
  nullable/implicit-usings, solution/project references).

## What still requires a Windows machine / green CI
- `dotnet build`, `dotnet test`, `dotnet publish` (x64 + arm64).
- Taskbar positioning, light/dark theme, Explorer restart, sleep/resume,
  installer install/uninstall — see checklist in `docs/BUILD.md`.

## To get a v1.0.0 release
1. Fix GitHub billing / enable Actions for the account.
2. Re-run "Build and Test" — expect green build + tests + artifacts.
3. Merge PR #1 to `main`, tag `v1.0.0`.
4. "Release" workflow builds the Inno Setup installer and publishes
   portable ZIPs + installer to GitHub Releases.

**No release was created, and no build artifacts are claimed, until CI passes.**
