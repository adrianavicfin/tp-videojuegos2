$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$repo = "adrianavicfin/tp-videojuegos2"
$projectNumber = 3
$owner = "DaamiAle"

Write-Host "1. Obteniendo lista de Issues existentes del repositorio..."
$existingIssues = gh issue list --repo $repo --state all --limit 50 --format json | ConvertFrom-Json

Write-Host "2. Vinculando cada Issue al GitHub Project..."

foreach ($issue in $existingIssues) {
    # Solo procesamos las issues activas (del Hito 1 números 14-21 y Hito 2 números 22-30)
    if ($issue.number -ge 14 -and $issue.number -le 30) {
        Write-Host "Agregando Issue #$($issue.number): $($issue.title)"
        
        $item = gh project item-add $projectNumber --owner $owner --url $issue.url --format json | ConvertFrom-Json
        
        # Si la issue está cerrada (Hito 1), la movemos a la columna Done
        if ($issue.state -eq "CLOSED") {
            gh project item-edit --project-id "PVT_kwHOBQcnPs4BiEbd" --id $item.id --field-id "PVTSSF_lAHOBQcnPs4BiEbdzhg9x10" --single-select-option-id "98236657"
        }
        # Si la issue está abierta (Hito 2), la movemos a la columna Backlog
        elseif ($issue.state -eq "OPEN") {
            gh project item-edit --project-id "PVT_kwHOBQcnPs4BiEbd" --id $item.id --field-id "PVTSSF_lAHOBQcnPs4BiEbdzhg9x10" --single-select-option-id "f75ad846"
        }
    }
}

Write-Host "¡Todas las Issues han sido agregadas y organizadas en el Project!"
