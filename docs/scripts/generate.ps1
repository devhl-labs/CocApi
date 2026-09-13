$packageVersion = "3.1.1"
$releaseNote = "Add new player and league group endpoints"

$properties = @(
    'packageName=CocApi.Rest'
    'packageTitle=CocApi.Rest'
    'apiName=CocApi'
    'validatable=false'
    'hideGenerationTimestamp=false'
    "packageVersion=$packageVersion"
    'packageAuthors=devhl'
    'packageCompany=devhl'
    'packageDescription=A wrapper for the Clash of Clans API'
    'packageTags=ClashOfClans SuperCell devhl'
    'modelPackage=Models'
    'apiPackage=Apis'
    'packageGuid=71B5E000-88E9-432B-BAEB-BB622EA7DC33'
    "dateTimeFormat=yyyyMMdd'T'HHmmss.fff'Z'"
    "dateFormat=yyyy'-'MM"
    "equatable=true"
    "useDateTimeForDate=true"
    "modelPropertySorting=legacy"
    "operationParameterSorting=legacy"
) -join ","

$global = @(
    'apiDocs=true'
    'modelDocs=true'
    'apiTests=true'
    'modelTests=true'
) -join ","

$generator = Resolve-Path -Path "$PSScriptRoot\..\..\..\openapi-generator\modules\openapi-generator-cli\target\openapi-generator-cli.jar"
$yml = Resolve-Path -Path "$PSScriptRoot\..\..\..\Clash-of-Clans-Swagger\swagger-3.0.yml"
$output = Resolve-Path -Path "$PSScriptRoot\..\.."
$templates = Resolve-Path -Path "$PSScriptRoot\..\templates"

java -jar $generator generate `
    -g csharp `
    -i $yml `
    -o $($output.Path) `
    --library generichost `
    --additional-properties $properties `
    --global-property $global `
    --git-host "github.com" `
    --git-repo-id "CocApi" `
    --git-user-id "devhl-labs" `
    -t $templates.Path `
    --release-note $releaseNote `
    --inject-model-vendor-extensions "ClanWar.clan.x-property-visibility=internal" `
    --inject-model-vendor-extensions "ClanWar.opponent.x-property-visibility=internal" `
    --inject-model-vendor-extensions "ClanWar.attacksPerMember.x-setter-visibility=internal" `
    --inject-model-vendor-extensions "ClanWarMember.mapPosition.x-property-visibility=internal" `
    --inject-model-vendor-extensions "Clan.memberList.x-property-visibility=internal" `
    --inject-model-vendor-extensions "Clan.members.x-property-visibility=internal"

$restPath = Resolve-Path -Path "$output\src\CocApi.Rest"
$testPath = Resolve-Path -Path "$output\src\CocApi.Rest.Test"
$apiDocPath = Resolve-Path -Path "$output\docs\apis"
$modelDocPath = Resolve-Path -Path "$output\docs\models"

$restFiles = $(Get-ChildItem -Path $restPath -Recurse)
$testFiles = $(Get-ChildItem -Path $testPath -Recurse)
$apiDocFiles = $(Get-ChildItem -Path $apiDocPath -Recurse)
$modelDocfiles = $(Get-ChildItem -Path $modelDocPath -Recurse)

$allFiles = @(
    ($apiDocFiles + $modelDocFiles) | Where-Object { -Not($_.PSIsContainer) }
    ($restFiles + $testFiles) | Where-Object {
        -Not($_.PSIsContainer) -and (
            $_.FullName.EndsWith(".cs") -or
            $_.FullName.EndsWith(".json") -or
            $_.FullName.EndsWith(".txt") -or
            $_.FullName.EndsWith(".csproj"))}
)

foreach ($file in $allFiles)
{
    $rawContent = $(Get-Content -Path $file.FullName)
    $originalContent = $rawContent -join "`n"
    $content = $rawContent -join "`r`n"

    if ($file.name.EndsWith("Api.cs")){
        $content=$content.Replace('> Get', '> Fetch')
        $content=$content.Replace("await Get", "await Fetch")
        $content=$content.Replace(" OnGet", " OnFetch")
        $content=$content.Replace(" AfterGet", " AfterFetch")
        $content=$content.Replace(" OnErrorGet", " OnErrorFetch")
        $content=$content.Replace("IGet", "IFetch")
    }

    if ($file.name.EndsWith("ApiTests.cs")){
        $content = $content.Replace(".GetClan", ".FetchClan")
        $content = $content.Replace(".GetCurrent", ".FetchCurrent")
        $content = $content.Replace(".GetPlayer", ".FetchPlayer")
        $content = $content.Replace(".GetLeague", ".FetchLeague")
        $content = $content.Replace(".GetWar", ".FetchWar")
        $content = $content.Replace(".GetLocation", ".FetchLocation")
        $content = $content.Replace(".GetCapitalRaidSeasonsAsync", ".FetchCapitalRaidSeasonsAsync")
        $content = $content.Replace(".GetBuilderBaseLeague", ".FetchBuilderBaseLeague")
        $content = $content.Replace(".GetCapitalLeague", ".FetchCapitalLeague")
    }

    if ($file.name -eq "Role.cs"){
        # here for legacy reasons
        $content = $content.Replace("Member = 1,", "Member = 0,")
        $content = $content.Replace("Admin = 2,", "Elder = 10,")
        $content = $content.Replace("CoLeader = 3,", "CoLeader = 20,")
        $content = $content.Replace("Leader = 4", "Leader = 30")
        $content = $content.Replace("return Role.Admin;", "return Role.Elder;")
        $content = $content.Replace("case Role.Admin:", "case Role.Elder:")
        $content = $content.Replace("if (value == Role.Admin)", "if (value == Role.Elder)")
    }

    if ($file.name -eq "Result.cs"){
        # here for legacy reasons
        $content = $content.Replace("Lose = 1,", "Lose = -1,")
        $content = $content.Replace("Tie = 2,", "Tie = 0,")
        $content = $content.Replace("Win = 3", "Win = 1,")
    }

    if (-Not([string]::IsNullOrWhiteSpace($content)) -and ($originalContent -cne $content)) {
        $isSet = $false
        # when Visual Studio is open, sometimes writing the content will fail, so do this in a loop
        do {
            try {
                Set-Content $file.PSPath $content -ErrorAction Stop
                $isSet = $true
            }
            catch {
                Write-Warning "Failed saving file $($file.FullName). Trying again..."
                Start-Sleep -Milliseconds 500
            }
        } while(!$isSet)
    }
    $content = $null
}

# bump the version for CocApi.Cache
$cacheProject = Resolve-Path -Path "$PSScriptRoot\..\..\src\CocApi.Cache\CocApi.Cache.csproj"
$content = Get-Content -Path $cacheProject.Path
$pattern = "(?s)<Version>(.*?)</Version>"
$content = $content -replace $pattern, "<Version>$packageVersion</Version>"
Write-Host
Set-Content $cacheProject.Path $content
