# Release Guide

## Quick Release

Run one of the scripts from the `Scripts` folder:

```powershell
.\Scripts\patch.ps1   # 1.0.0 → 1.0.1 (bug fixes)
.\Scripts\minor.ps1   # 1.0.0 → 1.1.0 (new features)
.\Scripts\major.ps1   # 1.0.0 → 2.0.0 (breaking changes)
```

The script will:
1. Update version in `.csproj`
2. Commit the change
3. Create and push a git tag
4. GitHub Actions handles the rest (build, test, publish to NuGet)

## CI/CD Behavior

| Trigger | Result |
|---------|--------|
| PR to `main` | Build + Test |
| Push to `main` | Publish preview (`1.0.0-preview.42`) |
| Push `v*` tag | Publish stable + GitHub Release |

## Setup (One-time)

Add `NUGET_API_KEY` to GitHub:
1. [nuget.org](https://www.nuget.org/) → API Keys → Create (Push scope)
2. GitHub repo → Settings → Secrets → Actions → New secret
