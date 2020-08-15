$ErrorActionPreference = "Stop"
cd ..\src\CocApi.Cache\Migrations
$count=(Get-ChildItem -File).Count
cd ..
dotnet ef migrations add Migration$count -s ../CocApi.Test
#dotnet ef database update -s ../CocApi.Test
Read-Host -Prompt "Press Enter to exit"