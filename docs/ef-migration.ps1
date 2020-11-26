Read-Host -Prompt "This will delete the cache and build a new migration.  Continue?"

if (Test-Path "..\src\CocApi.Test\bin\Debug\netcoreapp3.1\cocapi.db"){
    Remove-Item "..\src\CocApi.Test\bin\Debug\netcoreapp3.1\cocapi.db"
}

$count=(Get-ChildItem -File -Path ../src/CocApi.Cache/Migrations).Count
dotnet ef migrations add Migration$count -s ../src/CocApi.Test -p ../src/CocApi.Cache
Read-Host -Prompt "Press Enter to exit"