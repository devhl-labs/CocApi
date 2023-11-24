$packageVersion = "2.9.1"
$releaseNote = "Upgraded OpenApi"

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
) -join ","

$global = @(
    'apiDocs=true'
    'modelDocs=true'
    'apiTests=true'
    'modelTests=true'
) -join ","

$clanWarProperties = @"
    ClanWar:
      type: object
      properties:
"@

$clanWarPropertiesReplacement = @"
    ClanWar:
      type: object
      properties:
        # adding these for legacy reasons
        warTag:
            type: string
            readOnly: true
            nullable: true
        serverExpiration:
            type: date-time
            readOnly: true
"@

$clanWarRequiredProperies = @"
required:
      - attacksPerMember
"@

$clanWarRequiredProperiesReplacement = @"
required:
      - serverExpiration
      - attacksPerMember
"@


$generator = Resolve-Path -Path "$PSScriptRoot\..\..\..\openapi-generator\modules\openapi-generator-cli\target\openapi-generator-cli.jar"
$yml = Resolve-Path -Path "$PSScriptRoot\..\..\..\Clash-of-Clans-Swagger\swagger-3.0.yml"
$output = Resolve-Path -Path "$PSScriptRoot\..\.."
$templates = Resolve-Path -Path "$PSScriptRoot\..\templates"

$rawYml = $(Get-Content -Path $yml) -join "`r`n"
$rawYml = $rawYml.Replace($clanWarProperties, $clanWarPropertiesReplacement)
$rawYml = $rawYml.Replace($clanWarRequiredProperies, $clanWarRequiredProperiesReplacement)

# TODO: mark attacksPerMember as nullable
# but make it NOT be nullable in the appended-properties yaml, because we ensure that the property is there
# but SC does not

Set-Content "$PSScriptRoot\..\..\..\Clash-of-Clans-Swagger\swagger-3.0-appended-properties.yml" $rawYml
$appendedPropertiesYaml = Resolve-Path -Path "$PSScriptRoot\..\..\..\Clash-of-Clans-Swagger\swagger-3.0-appended-properties.yml"

java -jar $generator generate `
    -g csharp `
    -i $appendedPropertiesYaml `
    -o $($output.Path) `
    --library generichost `
    --additional-properties $properties `
    --global-property $global `
    --git-host "github.com" `
    --git-repo-id "CocApi" `
    --git-user-id "devhl-labs" `
    -t $templates.Path `
    --release-note $releaseNote

Remove-Item "$PSScriptRoot\..\..\..\Clash-of-Clans-Swagger\swagger-3.0-appended-properties.yml"

$membersProperty = @"
        /// <summary>
        /// Gets or Sets MemberList
        /// </summary>
        [JsonPropertyName("memberList")]
        public List<ClanMember> MemberList { get; }

        /// <summary>
        /// Gets or Sets Members
        /// </summary>
        [JsonPropertyName("members")]
        public int Members { get; }


"@

$membersEquals = @"
                (
                    MemberList == input.MemberList ||
                    MemberList != null &&
                    input.MemberList != null &&
                    MemberList.SequenceEqual(input.MemberList)
                ) && 

"@

$mapPosition = @"
        /// <summary>
        /// Gets or Sets MapPosition
        /// </summary>
        [JsonPropertyName("mapPosition")]
        public int MapPosition { get; }


"@

$membersConverter = @"
                        case "members":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                members = utf8JsonReader.GetInt32();
                            break;

"@

$apiKey = @"
                    List<TokenBase> tokenBaseLocalVars = new List<TokenBase>();

                    ApiKeyToken apiKeyTokenLocalVar = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokenBaseLocalVars.Add(apiKey);


"@

$tokenRateLimit = @"
                        else if (apiResponseLocalVar.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase tokenBaseLocalVar in tokenBaseLocalVars)
                                tokenBaseLocalVar.BeginRateLimit();

"@

$warClanNullChecks = @"
            if (attacks == null)
                throw new ArgumentNullException(nameof(attacks), "Property is required for class WarClan.");

            if (badgeUrls == null)
                throw new ArgumentNullException(nameof(badgeUrls), "Property is required for class WarClan.");

            if (clanLevel == null)
                throw new ArgumentNullException(nameof(clanLevel), "Property is required for class WarClan.");

            if (destructionPercentage == null)
                throw new ArgumentNullException(nameof(destructionPercentage), "Property is required for class WarClan.");

            if (expEarned == null)
                throw new ArgumentNullException(nameof(expEarned), "Property is required for class WarClan.");

            if (members == null)
                throw new ArgumentNullException(nameof(members), "Property is required for class WarClan.");

            if (name == null)
                throw new ArgumentNullException(nameof(name), "Property is required for class WarClan.");

            if (stars == null)
                throw new ArgumentNullException(nameof(stars), "Property is required for class WarClan.");

            if (tag == null)
                throw new ArgumentNullException(nameof(tag), "Property is required for class WarClan.");


"@


$warClanNullChecksReplacement = @"
            if (attacks == null)
                throw new ArgumentNullException(nameof(attacks), "Property is required for class WarClan.");

            if (badgeUrls == null)
                throw new ArgumentNullException(nameof(badgeUrls), "Property is required for class WarClan.");

            if (clanLevel == null)
                throw new ArgumentNullException(nameof(clanLevel), "Property is required for class WarClan.");

            if (destructionPercentage == null)
                throw new ArgumentNullException(nameof(destructionPercentage), "Property is required for class WarClan.");

            // if (expEarned == null)
            //     throw new ArgumentNullException(nameof(expEarned), "Property is required for class WarClan.");

            // if (members == null)
            //     throw new ArgumentNullException(nameof(members), "Property is required for class WarClan.");

            // if (name == null)
            //     throw new ArgumentNullException(nameof(name), "Property is required for class WarClan.");

            if (stars == null)
                throw new ArgumentNullException(nameof(stars), "Property is required for class WarClan.");

            // if (tag == null)
            //     throw new ArgumentNullException(nameof(tag), "Property is required for class WarClan.");


"@


$clanServerExpirtationEquals = @"
                (
                    ServerExpiration == input.ServerExpiration ||
                    (ServerExpiration != null &&
                    ServerExpiration.Equals(input.ServerExpiration))
                ) && 

"@


$serverExpirationEquals = @"
                (
                    ServerExpiration == input.ServerExpiration ||
                    ServerExpiration.Equals(input.ServerExpiration)
                ) && 

"@

$membersNullCheck = @"
            if (clan.MemberList == null)
                throw new ArgumentNullException(nameof(clan.MemberList), "Property is required for class Clan.");

"@

$membersNullCheckReplacement = @"
            if (clan.Members == null)
                throw new ArgumentNullException(nameof(clan.Members), "Property is required for class Clan.");

"@

$restPath = Resolve-Path -Path "$output\src\CocApi.Rest"
$testPath = Resolve-Path -Path "$output\src\CocApi.Rest.Test"
$apiDocPath = Resolve-Path -Path "$output\docs\apis"
$modelDocPath = Resolve-Path -Path "$output\docs\models"

$restFiles = $(Get-ChildItem -Path $restPath -Recurse)
$testFiles = $(Get-ChildItem -Path $testPath -Recurse)
$apiDocFiles = $(Get-ChildItem -Path $apiDocPath -Recurse)
$modelDocfiles = $(Get-ChildItem -Path $modelDocPath -Recurse)

$allDocFiles = $($apiDocFiles + $modelDocFiles) |
    Where-Object {
        -Not($_.PSIsContainer) -and (
            $_.FullName -match ".md" -or
            $_.FullName -match "")}

$allCodeFiles = $($restFiles + $testFiles) |
    Where-Object {
        -Not($_.PSIsContainer) -and (
            $_.FullName.EndsWith(".cs") -or
            $_.FullName.EndsWith(".json") -or
            $_.FullName.EndsWith(".txt") -or
            $_.FullName.EndsWith(".csproj"))}

foreach ($file in $allDocFiles)
{
    $rawContent = $(Get-Content -Path $file.FullName)
    $originalContent = $rawContent -join "`n"
    $content = $rawContent -join "`r`n"

    if ($originalContent -cne $content){
        try {
            Set-Content $file.FullName $content -ErrorAction Stop
        }
        catch {
            Write-Warning "Failed saving file $($file.FullName). Trying again in two seconds."
            Start-Sleep -Seconds 2
            Set-Content $file.FullName $content
        }
    }
}

foreach ($file in $allCodeFiles)
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

    if ($file.name -eq "WarClan.cs"){
        $content=$content.Replace($warClanNullChecks, $warClanNullChecksReplacement)
        $content=$content.Replace(", expEarned.Value,", ", expEarned.GetValueOrDefault(),")
    }

    if ($file.name -eq "Clan.cs"){
        $content = $content.Replace("/// <param name=`"memberList`">memberList</param>`r`n        ", "")
        $content = $content.Replace("MemberList = memberList;`r`n            ", "")
        $content = $content.Replace("List<ClanMember> memberList, int members", "List<ClanMember> members")
        $content = $content.Replace($membersProperty, "")
        $content = $content.Replace("hashCode = (hashCode * 59) + MemberList.GetHashCode();`r`n                ", "")
        $content = $content.Replace($membersEquals, "")
        $content = $content.Replace("sb.Append(`"  MemberList: `").Append(MemberList).Append(`"\n`");`r`n            ", '')
        $content = $content.Replace($membersConverter, "")
        $content = $content.Replace("int members = default;`r`n            ", '')
        $content = $content.Replace("memberList.Value!, members.Value!.Value!,", "memberList,")

        $content = $content.Replace("JsonSerializer.Serialize(writer, clan.MemberList, jsonSerializerOptions);", "JsonSerializer.Serialize(writer, clan.Members, jsonSerializerOptions);")
        $content = $content.Replace("writer.WriteNumber(`"members`", clan.Members);`r`n            ", "")

        $content = $content.Replace("if (members == null)", "if (memberList == null)")
        $content = $content.Replace("ArgumentNullException(nameof(members)", "ArgumentNullException(nameof(memberList)")
        $content = $content.Replace("case `"clanBuilderBasePoints`":", "case `"clanBuilderBasePoints`":`r`n                        case `"clanVersusPoints`": // legacy property")
        $content = $content.Replace("case `"requiredBuilderBaseTrophies`":", "case `"requiredBuilderBaseTrophies`":`r`n                        case `"requiredVersusTrophies`": // legacy property")
        $content = $content.Replace($membersNullCheck, $membersNullCheckReplacement)
        $content = $content.Replace("writer.WriteNumber(`"members`", clan.Members);", "writer.WriteNumber(`"members`", clan.Members.Count);")
    }

    if ($file.name -eq "ClanMember.cs"){
        $content = $content.Replace("case `"builderBaseTrophies`":", "case `"builderBaseTrophies`":`r`n                        case `"versusTrophies`": // legacy property")
    }

    if ($file.name -eq "ClanBuilderBaseRanking.cs"){
        $content = $content.Replace("case `"clanBuilderBasePoints`":", "case `"clanBuilderBasePoints`":`r`n                        case `"clanVersusPoints`": // legacy property")
    }

    if ($file.name -eq "Player.cs"){
        $content = $content.Replace("case `"builderBaseTrophies`":", "case `"builderBaseTrophies`":`r`n                        case `"versusTrophies`": // legacy property")
    }

    if ($file.name -eq "PlayerLegendStatistics.cs"){
        $content = $content.Replace("case `"previousBuilderBaseSeason`":", "case `"previousBuilderBaseSeason`":`r`n                        case `"previousVersusSeason`": // legacy property")
        $content = $content.Replace("case `"bestBuilderBaseSeason`":", "case `"bestBuilderBaseSeason`":`r`n                        case `"bestVersusSeason`": // legacy property")
    }

    if ($file.name -eq "PlayerBuilderBaseRanking.cs"){
        $content = $content.Replace("case `"builderBaseTrophies`":", "case `"builderBaseTrophies`":`r`n                        case `"versusTrophies`": // legacy property")
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

    if ($file.name -eq "WarType.cs"){
        $content = $content.Replace("Sccwl", "SCCWL")
    }

    if ($file.name -eq "ClanWarMember.cs"){
        $content = $content.Replace($mapPosition, "")
        $content = $content.Replace("MapPosition = mapPosition;", "RosterPosition = mapPosition; // this is intentional. The MapPosition will be caculated in ClanWar#OnCreated")
    }

    if ($file.name -eq "ClanWar.cs"){
        $content = $content.Replace("public WarClan Clan { get; }", "public WarClan Clan { get; private set; }")
        $content = $content.Replace("public WarClan Opponent { get; }", "public WarClan Opponent { get; private set; }")
        $content = $content.Replace("public int AttacksPerMember { get; }", "public int AttacksPerMember { get; private set; }")
        $content = $content.Replace($clanServerExpirtationEquals, "")
        $content = $content.Replace("hashCode = (hashCode * 59) + ServerExpiration.GetHashCode();`r`n                ", "")
        $content = $content.Replace($serverExpirationEquals, "")

        # this can be removed in the next update, just to ensure we can deserialize the old json data which wont have the serverExpiration value
        $content = $content.Replace("throw new ArgumentNullException(nameof(serverExpiration), `"Property is required for class ClanWar.`");", "serverExpiration = new DateTime(2023, 05, 01, 1, 1, 1, 1, 1);")
        $content = $content.Replace("throw new ArgumentNullException(nameof(attacksPerMember), `"Property is required for class ClanWar.`");", "attacksPerMember = 1; // cwl war")
    }

    if ($file.name -eq "DeveloperApi.cs"){
        $content = $content.Replace($apiKey, "")
        $content = $content.Replace($tokenRateLimit, "")
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
