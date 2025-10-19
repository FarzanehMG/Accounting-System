$curDir = $PSScriptRoot
$solutionDir = (Split-Path $PSScriptRoot -Parent)

# Paths
$projectFile = "$solutionDir/API/API.csproj"
$namespace = "GenerateApi"
$nswagFile = "$curDir/Api.nswag"

# NSwag executable
$nswagExe = "${Env:ProgramFiles(x86)}/Rico Suter/NSwagStudio/Net80/dotnet-nswag.exe"
$variables = "/variables:namespace=$namespace,projectFile=$projectFile"

Write-Host "Running NSwag..."
Write-Host "Using NSwag file: $nswagFile"
Write-Host "Using project file: $projectFile"
Write-Host "Namespace: $namespace"
Write-Host ""

# Run NSwag
& "$nswagExe" run $nswagFile $variables

Write-Host ""
Write-Host "NSwag code generation completed successfully."
