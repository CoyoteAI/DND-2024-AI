# Test Ollama Connection and Qwen Model
# Run this script to verify your Ollama setup

Write-Host "=== Ollama Connection Test ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if Ollama is running
Write-Host "1. Testing Ollama API connection..." -ForegroundColor Yellow
try {
    $version = Invoke-RestMethod -Uri "http://localhost:11434/api/version" -Method Get -ErrorAction Stop
    Write-Host "   ✓ Ollama is running!" -ForegroundColor Green
    Write-Host "   Version: $($version.version)" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ Cannot connect to Ollama!" -ForegroundColor Red
    Write-Host "   Make sure Ollama is running (try 'ollama serve' in a terminal)" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 2: List available models
Write-Host "2. Checking available models..." -ForegroundColor Yellow
try {
    $tags = Invoke-RestMethod -Uri "http://localhost:11434/api/tags" -Method Get -ErrorAction Stop
    Write-Host "   ✓ Models found:" -ForegroundColor Green
    foreach ($model in $tags.models) {
        Write-Host "      - $($model.name)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Could not retrieve models" -ForegroundColor Red
}

Write-Host ""

# Test 3: Check for qwen3:4b specifically
Write-Host "3. Checking for 'qwen3:4b' model..." -ForegroundColor Yellow
$modelFound = $false
foreach ($model in $tags.models) {
    if ($model.name -eq "qwen3:4b") {
        $modelFound = $true
        break
    }
}

if ($modelFound) {
    Write-Host "   ✓ qwen3:4b is installed!" -ForegroundColor Green
} else {
    Write-Host "   ✗ qwen3:4b NOT found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Available alternatives:" -ForegroundColor Yellow
    Write-Host "   - qwen2.5:3b  (recommended for smaller, faster model)" -ForegroundColor Gray
    Write-Host "   - qwen:4b     (alternative 4B parameter model)" -ForegroundColor Gray
    Write-Host "   - qwen2.5:7b  (larger, more capable model)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   To install a model, run:" -ForegroundColor Yellow
    Write-Host "   ollama pull qwen2.5:3b" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   Then update appsettings.json to use the correct model name." -ForegroundColor Yellow
}

Write-Host ""

# Test 4: Try a simple generation
Write-Host "4. Testing generation with available model..." -ForegroundColor Yellow

# Use the first available model for testing
$testModel = $tags.models[0].name
Write-Host "   Using model: $testModel" -ForegroundColor Gray

$testRequest = @{
    model = $testModel
    prompt = "Say hello in one sentence."
    stream = $false
    options = @{
        temperature = 0.7
        num_predict = 50
    }
} | ConvertTo-Json

try {
    Write-Host "   Sending test request (this may take a moment)..." -ForegroundColor Gray
    $response = Invoke-RestMethod -Uri "http://localhost:11434/api/generate" `
                                   -Method Post `
                                   -Body $testRequest `
                                   -ContentType "application/json" `
                                   -TimeoutSec 30 `
                                   -ErrorAction Stop

    Write-Host "   ✓ Generation successful!" -ForegroundColor Green
    Write-Host "   Response: $($response.response)" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ Generation failed!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Make sure the model name in appsettings.json matches an installed model" -ForegroundColor Gray
Write-Host "2. Run your D&D AI app in Debug mode (F5 in Visual Studio)" -ForegroundColor Gray
Write-Host "3. Check the Output window for [Qwen] debug messages" -ForegroundColor Gray
