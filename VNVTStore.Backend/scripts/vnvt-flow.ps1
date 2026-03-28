# VNVTStore Development Flow Automation
# Usage: 
#   .\scripts\vnvt-flow.ps1 -Action start   : Pull from staging and sync
#   .\scripts\vnvt-flow.ps1 -Action check   : Build and Test everything
#   .\scripts\vnvt-flow.ps1 -Action finish  : Check, then Merge and Push to staging

param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "check", "finish")]
    [string]$Action
)

$BackendPath = Resolve-Path "$PSScriptRoot\.."
$RootPath = Resolve-Path "$BackendPath\.."
$FrontendPath = "$RootPath\VNVTStore.Frontend"
$SolutionPath = "$BackendPath\VNVTStore.slnx"

function Write-Step {
    param($Message)
    Write-Host "`n>>> $Message" -ForegroundColor Cyan
}

function Run-Check {
    Write-Step "Building and Testing VNVTStore..."

    # 1. Backend Build
    Write-Host "--- Backend Build ---"
    dotnet build $SolutionPath
    if ($LASTEXITCODE -ne 0) { throw "Backend Build Failed!" }

    # 2. Backend Test
    Write-Host "--- Backend Test ---"
    dotnet test $SolutionPath --no-build
    if ($LASTEXITCODE -ne 0) { throw "Backend Tests Failed!" }

    # 3. Frontend Build
    Write-Host "--- Frontend Build ---"
    Push-Location $FrontendPath
    npm run build
    $feExit = $LASTEXITCODE
    Pop-Location
    if ($feExit -ne 0) { throw "Frontend Build Failed!" }

    Write-Host "`n[SUCCESS] All checks passed!" -ForegroundColor Green
    return $true
}

try {
    switch ($Action) {
        "start" {
            Write-Step "Syncing with Staging..."
            git fetch origin staging
            git checkout staging
            git pull origin staging
            Write-Host "Staging is up to date." -ForegroundColor Green
            # Switch back to previous branch if any, or stay on staging if user wants
            Write-Host "Please create or switch to your feature branch now."
        }

        "check" {
            Run-Check
        }

        "finish" {
            Run-Check

            Write-Step "Merging into Staging..."
            $currentBranch = (git branch --show-current)
            if ($currentBranch -eq "staging") { throw "You are already on staging. Commit your changes first." }

            git checkout staging
            git merge $currentBranch --no-edit
            if ($LASTEXITCODE -ne 0) { throw "Merge Failed! Resolve conflicts manually." }

            Write-Step "Pushing to Staging..."
            git push origin staging
            Write-Host "Successfully merged and pushed to staging!" -ForegroundColor Green
            
            git checkout $currentBranch
        }
    }
}
catch {
    Write-Host "`n[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
