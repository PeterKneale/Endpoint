param (
    $resourceBaseName="todoApp$( Get-Random -Maximum 1000)",
    $location='eastus'
)

Write-Host 'Creating resource group' -ForegroundColor Green
az group create -l westus -n $resourceBaseName

Write-Host 'Creating Azure resources' -ForegroundColor Green
az deployment group create --resource-group $resourceBaseName --template-file main.bicep

Write-Host 'Building API project' -ForegroundColor Green
dotnet build ..\src\TodoApp.Api\TodoApp.Api.csproj

Write-Host 'Packaging API project for deployment' -ForegroundColor Green
dotnet publish ..\src\TodoApp.Api\TodoApp.Api.csproj --self-contained -r win-x86 -o ..\publish

try {
    dotnet tool install --global Zipper --version 1.0.1
} catch {
    # no work to do
}

try {
    Remove-Item -Path deployment.zip -Force
}
catch {
    # no work to do
}

# Compress-Archive -Path ..\publish\*.* -DestinationPath deployment.zip -Force
zipper compress -i ..\publish\ -o deployment.zip

Write-Host 'Deploying API project to Azure Web Apps' -ForegroundColor Green
az webapp deploy -n "$($resourceBaseName)web" -g $resourceBaseName --src-path .\deployment.zip
