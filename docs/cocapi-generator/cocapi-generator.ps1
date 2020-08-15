$ErrorActionPreference = "Stop"
$deleted = $env:temp + "\Deleted " + (get-date -Format "MMddyyyy hhmmss")

Write-Output "Moving folders to $deleted"
New-Item -Path $deleted -ItemType directory
if (Test-Path -Path ..\..\src\CocApi)
{
    Move-Item -Path ..\..\src\CocApi -Destination $deleted
}
cmd /c start /wait java -jar $env:openapi5 generate -g csharp-netcore -i ..\..\..\Clash-of-Clans-Swagger\swagger.yml -c generator-config.json -o ..\..\src\CocApi -t templates --global-property apiTests,modelTests | Out-Null
Set-Location ..\..\src\CocApi
Remove-Item -Path .gitignore
Remove-Item -Path CocApi.sln
Remove-Item -Path git_push.sh
Set-Location .\src\CocApi
Move-Item -Path Api -Destination ..\..
Move-Item -Path Client -Destination ..\..
Move-Item -Path Model -Destination ..\..
Move-Item -Path CocApi.csproj -Destination ..\..
if (Test-Path -Path $deleted\CocApi){
    Move-Item -Path $deleted\CocApi\ManualAdditions -Destination ..\..
}
Set-Location ..\..
Move-Item -Path src -Destination $deleted

Set-Location ..\..\docs\cocapi-generator
Read-Host -Prompt "Press Enter to exit"