$ErrorActionPreference = "Stop"

Read-Host -Prompt "This will delete the cache and build a new migration.  Continue?"

if (Test-Path "..\src\CocApi.Test\bin\Debug\netcoreapp3.1\cocapi.db"){
    Remove-Item "..\src\CocApi.Test\bin\Debug\netcoreapp3.1\cocapi.db"
}
Set-Location ..\src\CocApi.Cache\Migrations
$count=(Get-ChildItem -File).Count
Set-Location ..
dotnet ef migrations add Migration$count -s ../CocApi.Test
#dotnet ef database update -s ../CocApi.Test
Read-Host -Prompt "Press Enter to exit"