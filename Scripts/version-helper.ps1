# Shared version helper functions

function Get-CurrentVersion {
    $csprojPath = "$PSScriptRoot\..\Source\EntityFramework.Extensions.GetItems\EntityFramework.Extensions.GetItems.csproj"
    $content = Get-Content $csprojPath -Raw
    if ($content -match '<Version>(\d+)\.(\d+)\.(\d+)</Version>') {
        return @{
            Major = [int]$Matches[1]
            Minor = [int]$Matches[2]
            Patch = [int]$Matches[3]
            Full = "$($Matches[1]).$($Matches[2]).$($Matches[3])"
        }
    }
    throw "Could not parse version from csproj"
}

function Set-Version {
    param([string]$NewVersion)
    
    $csprojPath = "$PSScriptRoot\..\Source\EntityFramework.Extensions.GetItems\EntityFramework.Extensions.GetItems.csproj"
    $content = Get-Content $csprojPath -Raw
    $content = $content -replace '<Version>\d+\.\d+\.\d+</Version>', "<Version>$NewVersion</Version>"
    Set-Content $csprojPath $content -NoNewline
}

function Publish-Release {
    param(
        [string]$BumpType  # "major", "minor", or "patch"
    )
    
    # Get current version
    $current = Get-CurrentVersion
    Write-Host "Current version: $($current.Full)" -ForegroundColor Cyan
    
    # Calculate new version
    switch ($BumpType) {
        "major" { $newVersion = "$($current.Major + 1).0.0" }
        "minor" { $newVersion = "$($current.Major).$($current.Minor + 1).0" }
        "patch" { $newVersion = "$($current.Major).$($current.Minor).$($current.Patch + 1)" }
    }
    
    Write-Host "New version: $newVersion" -ForegroundColor Green
    
    # Confirm
    $confirm = Read-Host "Proceed with release v$newVersion? (y/n)"
    if ($confirm -ne 'y') {
        Write-Host "Aborted." -ForegroundColor Yellow
        return
    }
    
    # Update csproj
    Set-Version $newVersion
    Write-Host "Updated csproj" -ForegroundColor Gray
    
    # Git operations
    git add -A
    git commit -m "Release v$newVersion"
    git tag "v$newVersion"
    git push origin main
    git push origin "v$newVersion"
    
    Write-Host "`nReleased v$newVersion" -ForegroundColor Green
    Write-Host "GitHub Actions will now build and publish to NuGet." -ForegroundColor Cyan
}

