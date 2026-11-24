# Build / CI Status (updated 2026-11-24)

## Summary

| Item | Status |
|---|---|
| Source code | Complete (24 C# files, ~2,200 lines incl. tests/docs) |
| GitHub repository | Code pushed to `thebinsultan/speedmeter` (see note below) |
| GitHub Actions build/test | **Blocked by GitHub account billing lock** — job never starts |
| `dotnet build` / `dotnet test` locally | **Not possible** — sandbox is Linux without .NET SDK |
| v1.0.0 tag / GitHub Release | **Not created** (only after a green build, per requirements) |
| Portable exe / installer artifacts | **None produced** |

## Evidence (Actions)

Three workflow runs were triggered on PR #1; every one shows the same annotation
on the `build` job, after ~2–3 seconds:

> "The job was not started because your account is locked due to a billing issue."

Runs: 45353040882, 45353117969, 45354043181. No step ever executed, so no
compiler output exists for these runs — this is an account-level Actions block,
not a build failure caused by the code.

## What has actually been verified

| Check | Method | Result |
|---|---|---|
| Syntax of all C# files (app, Core, tests) | tree-sitter C# grammar, full AST walk for `ERROR`/`MISSING` nodes | 0 errors |
| Project/manifest XML | `xml.etree` parse of all 3 csproj, `Directory.Build.props`, `app.manifest` | valid |
| Core algorithms vs. the unit tests | unit-test expectations re-derived with a 1:1 Python port of `UnitFormatter`, `SpeedCalculator`, `UsageTracker` | all pass |
| Type/API correctness | manual review; four real defects found and fixed (`UseWinForms` → `UseWindowsForms`, SDK re-import in `Directory.Build.props`, `Version` `>` operator, `FontFamily` ambiguity) | fixed, unverified by a compiler |

## Not verified (needs Windows + a working .NET SDK)

`dotnet restore/build/test/publish`, actual win-x64 and win-arm64 binaries,
installer compilation, taskbar positioning, light/dark theme, DPI scaling,
sleep/resume, Explorer restart, install/uninstall, RAM/CPU targets.

A manual checklist is at the end of [BUILD.md](BUILD.md).

## To produce v1.0.0

1. Resolve the GitHub billing lock (or make Actions available for the account).
2. Re-run **Build and Test** on PR #1 — it restores, builds, tests, publishes
   x64 + arm64 portable exes and uploads them as artifacts.
3. Merge PR #1 into `main`.
4. Push tag `v1.0.0` — the **Release** workflow re-builds, compiles the Inno Setup
   installer with `installer/NetSpeedMonitor.iss`, and publishes a GitHub Release
   with `NetSpeedMonitor-portable-win-x64.zip`, `...-win-arm64.zip` and
   `NetSpeedMonitor-Setup-1.0.0.exe`.

Only after step 4 do the release/download links from the README become real.

## Repository naming note

The connected GitHub token can create commits and pull requests but is **not
permitted to create repositories** (`POST /user/repos` → `403 Resource not
accessible by integration`), so `thebinsultan/NetSpeed-Monitor` could not be
created. The complete code therefore lives on a branch in the existing
`thebinsultan/speedmeter` repository. To get the requested name, either:

- rename `speedmeter` → `NetSpeed-Monitor` in *Settings → General → Repository name*, or
- create an empty `NetSpeed-Monitor` repo and push this branch's commits to `main`.

The code, workflows, installer paths and the update-checker URL
(`api.github.com/repos/thebinsultan/NetSpeed-Monitor/releases/latest`) already
assume the final name, so no source changes are required after renaming.
