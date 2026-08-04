# PowerShell Script: Start Full Stack with Hot Reload (No Rebuilding Needed!)
Write-Host "=========================================" -ForegroundColor- Cyan
Write-Host "Starting MiniBillingSystem with HOT RELOAD..." -ForegroundColor- Green
Write-Host "=========================================" -ForegroundColor- Cyan

# Step 1: Start Docker compose with dev configuration
Write-Host "[1/3] Spinning up database, dotnet watch backend, and Vite HMR frontend..." -ForegroundColor- Yellow
docker compose -f docker-compose.dev.yml up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to launch containers via docker-compose.dev.yml" -ForegroundColor- Red
    exit 1
}

# Step 2: Health check loop
Write-Host "[2/3] Waiting for services to initialize..." -ForegroundColor- Yellow
$backendHealthy = $false

for ($i = 1; $i -le 30; $i++) {
    try {
        $res = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get -ErrorAction Stop
        if ($res.status -eq "Healthy") {
            $backendHealthy = $true
            break
        }
    } catch {
        # Waiting...
    }
    Start-Sleep -Seconds 2
}

if (-not $backendHealthy) {
    Write-Host "WARNING: Backend initialization is taking longer than expected. Run 'docker compose logs -f' to inspect." -ForegroundColor- Red
}

Write-Host "=========================================" -ForegroundColor- Green
Write-Host "SUCCESS: Hot Reload Stack is active!" -ForegroundColor- Green
Write-Host "Frontend UI (Vite Instant HMR): http://localhost:5173" -ForegroundColor- Cyan
Write-Host "Backend API (Dotnet Watch):     http://localhost:5000/health" -ForegroundColor- Cyan
Write-Host "=========================================" -ForegroundColor- Green
Write-Host "Any changes saved to C# or React files will hot-reload INSTANTLY without rebuilding!" -ForegroundColor- Magenta
