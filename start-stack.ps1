Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Starting MiniBillingSystem Stack..." -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

docker compose up -d --build

Write-Host "`nWaiting for database, backend, and frontend health checks..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$healthy = $false

while ($attempt -lt $maxAttempts) {
    Start-Sleep -Seconds 2
    $attempt++
    
    try {
        $backendHealth = Invoke-RestMethod -Uri "http://localhost:5000/health" -ErrorAction SilentlyContinue
        $frontendHealth = Invoke-RestMethod -Uri "http://localhost:80/healthz" -ErrorAction SilentlyContinue

        if ($backendHealth.status -eq "Healthy" -and $frontendHealth.status -eq "Healthy") {
            $healthy = $true
            break
        }
    } catch {
        # Retry until healthy
    }
    
    Write-Host "." -NoNewline
}

Write-Host ""
if ($healthy) {
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "SUCCESS: Stack is fully initialized and healthy!" -ForegroundColor Green
    Write-Host "Frontend: http://localhost:80" -ForegroundColor Green
    Write-Host "Backend API & Health: http://localhost:5000/health" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
} else {
    Write-Host "ERROR: Stack health checks failed or timed out." -ForegroundColor Red
    docker compose ps
    exit 1
}
