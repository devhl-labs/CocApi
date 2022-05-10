$packageVersion = "2.0.0-preview1.14.14"
$releaseNote = "Moved rest methods to CocApi.Rest. Now using automation to generate rest methods from openapi yaml."

$properties = @(
    'packageName=CocApi.Rest',
    'packageTitle=CocApi.Rest',
    'apiName=CocApi',
    'targetFramework=net6.0',
    'validatable=false',
    'nullableReferenceTypes=true',
    'hideGenerationTimestamp=false',
    "packageVersion=$packageVersion",
    'packageAuthors=devhl',
    'packageCompany=devhl',
    'packageDescription=A wrapper for the Clash of Clans API',
    'packageTags=ClashOfClans SuperCell devhl'
    'modelPackage=Models',
    'apiPackage=BaseApis',
    'derivedApiPackage=Apis'
) -join ","

$global = @(
    'apiDocs=true',
    'modelDocs=true',
    'apiTests=true',
    'modelTests=true'
) -join ","

$generator = Resolve-Path -Path "$PSScriptRoot\..\..\..\openapi-generator\modules\openapi-generator-cli\target\openapi-generator-cli.jar"
$yml = Resolve-Path -Path "$PSScriptRoot\..\..\..\Clash-of-Clans-Swagger\swagger-3.0.yml"
$output = Resolve-Path -Path "$PSScriptRoot\..\.."
$templates = Resolve-Path -Path "$PSScriptRoot\..\templates"

java -jar $generator generate `
    -g csharp-netcore `
    -i $yml `
    -o $($output.Path) `
    --library generichost `
    --additional-properties $properties `
    --global-property $global `
    --git-host "github.com" `
    --git-repo-id "CocApi" `
    --git-user-id "devhl-labs" `
    -t $templates.Path `
    --release-note $releaseNote

$warClanConstructor = @"
        /// <summary>
        /// Initializes a new instance of the <see cref="WarClan" /> class.
        /// </summary>
        /// <param name="attacks">attacks</param>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="destructionPercentage">destructionPercentage</param>
        /// <param name="expEarned">expEarned</param>
        /// <param name="members">members</param>
        /// <param name="name">name</param>
        /// <param name="stars">stars</param>
        /// <param name="tag">tag</param>
        [JsonConstructor]
        internal WarClan(int attacks, BadgeUrls badgeUrls, int clanLevel, float destructionPercentage, int expEarned, List<ClanWarMember> members, string name, int stars, string tag)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (destructionPercentage == null)
                throw new ArgumentNullException("destructionPercentage is a required property for WarClan and cannot be null.");

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for WarClan and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for WarClan and cannot be null.");

            if (badgeUrls == null)
                throw new ArgumentNullException("badgeUrls is a required property for WarClan and cannot be null.");

            if (clanLevel == null)
                throw new ArgumentNullException("clanLevel is a required property for WarClan and cannot be null.");

            if (attacks == null)
                throw new ArgumentNullException("attacks is a required property for WarClan and cannot be null.");

            if (stars == null)
                throw new ArgumentNullException("stars is a required property for WarClan and cannot be null.");

            if (expEarned == null)
                throw new ArgumentNullException("expEarned is a required property for WarClan and cannot be null.");

            if (members == null)
                throw new ArgumentNullException("members is a required property for WarClan and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            Attacks = attacks;
            BadgeUrls = badgeUrls;
            ClanLevel = clanLevel;
            DestructionPercentage = destructionPercentage;
            ExpEarned = expEarned;
            Members = members;
            Name = name;
            Stars = stars;
            Tag = tag;
        }


"@

$clanConstructor = @"
        /// <summary>
        /// Initializes a new instance of the <see cref="Clan" /> class.
        /// </summary>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="clanPoints">clanPoints</param>
        /// <param name="clanVersusPoints">clanVersusPoints</param>
        /// <param name="description">description</param>
        /// <param name="isWarLogPublic">isWarLogPublic</param>
        /// <param name="labels">labels</param>
        /// <param name="memberList">memberList</param>
        /// <param name="members">members</param>
        /// <param name="name">name</param>
        /// <param name="requiredTrophies">requiredTrophies</param>
        /// <param name="tag">tag</param>
        /// <param name="warLeague">warLeague</param>
        /// <param name="warWinStreak">warWinStreak</param>
        /// <param name="warWins">warWins</param>
        /// <param name="chatLanguage">chatLanguage</param>
        /// <param name="location">location</param>
        /// <param name="type">type</param>
        /// <param name="warFrequency">warFrequency</param>
        /// <param name="warLosses">warLosses</param>
        /// <param name="warTies">warTies</param>
        [JsonConstructor]
        internal Clan(BadgeUrls badgeUrls, int clanLevel, int clanPoints, int clanVersusPoints, string description, bool isWarLogPublic, List<Label> labels, List<ClanMember> memberList, int members, string name, int requiredTrophies, string tag, WarLeague warLeague, int warWinStreak, int warWins, Language? chatLanguage = default, Location? location = default, RecruitingType? type = default, WarFrequency? warFrequency = default, int? warLosses = default, int? warTies = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (warLeague == null)
                throw new ArgumentNullException("warLeague is a required property for Clan and cannot be null.");

            if (memberList == null)
                throw new ArgumentNullException("memberList is a required property for Clan and cannot be null.");

            if (requiredTrophies == null)
                throw new ArgumentNullException("requiredTrophies is a required property for Clan and cannot be null.");

            if (clanVersusPoints == null)
                throw new ArgumentNullException("clanVersusPoints is a required property for Clan and cannot be null.");

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for Clan and cannot be null.");

            if (isWarLogPublic == null)
                throw new ArgumentNullException("isWarLogPublic is a required property for Clan and cannot be null.");

            if (clanLevel == null)
                throw new ArgumentNullException("clanLevel is a required property for Clan and cannot be null.");

            if (warWinStreak == null)
                throw new ArgumentNullException("warWinStreak is a required property for Clan and cannot be null.");

            if (warWins == null)
                throw new ArgumentNullException("warWins is a required property for Clan and cannot be null.");

            if (clanPoints == null)
                throw new ArgumentNullException("clanPoints is a required property for Clan and cannot be null.");

            if (labels == null)
                throw new ArgumentNullException("labels is a required property for Clan and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for Clan and cannot be null.");

            if (members == null)
                throw new ArgumentNullException("members is a required property for Clan and cannot be null.");

            if (description == null)
                throw new ArgumentNullException("description is a required property for Clan and cannot be null.");

            if (badgeUrls == null)
                throw new ArgumentNullException("badgeUrls is a required property for Clan and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            BadgeUrls = badgeUrls;
            ClanLevel = clanLevel;
            ClanPoints = clanPoints;
            ClanVersusPoints = clanVersusPoints;
            Description = description;
            IsWarLogPublic = isWarLogPublic;
            Labels = labels;
            MemberList = memberList;
            Members = members;
            Name = name;
            RequiredTrophies = requiredTrophies;
            Tag = tag;
            WarLeague = warLeague;
            WarWinStreak = warWinStreak;
            WarWins = warWins;
            ChatLanguage = chatLanguage;
            Location = location;
            Type = type;
            WarFrequency = warFrequency;
            WarLosses = warLosses;
            WarTies = warTies;
        }


"@


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

$memberListHash = @"
                hashCode = (hashCode * 59) + MemberList.GetHashCode();

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
                            members = reader.GetInt32();
                            break;

"@

$project = Resolve-Path -Path "$output\src\CocApi.Rest"
$projectTest = Resolve-Path -Path "$output\src\CocApi.Rest.Test"

$files = $(Get-ChildItem -Path $project -Recurse)
$files += $(Get-ChildItem -Path $projectTest -Recurse)

foreach ($file in $files)
{
    if ($file.PSIsContainer){
        continue
    }

    $content=Get-Content -Path $file.FullName -Raw
    $originalContent = $content

    if (-Not($content)){
        continue
    }

    if ($file.DirectoryName -match "CocApi.Rest" -and -Not($file.DirectoryName -match "CocApi.Rest.Test") -and -Not($file.name -match ".Manual.") -and -Not($content -match "`r`n")){
        $content = $content.Replace("`n","`r`n")
    } 
    elseif ($file.DirectoryName -match "CocApi.Rest.Test" -and -Not($content -match "`r`n")){
        $content = $content.Replace("`n","`r`n")
    }

    $content=$content.Replace("WithHttpInfoAsync(", "ResponseAsync(")

    if ($file.name.EndsWith("Api.cs")){
        $content=$content.Replace('> Get', '> Fetch')
        $content=$content.Replace("await Get", "await Fetch")
        $content=$content.Replace(" OnGet", " OnFetch")
        $content=$content.Replace(" AfterGet", " AfterFetch")
        $content=$content.Replace(" OnErrorGet", " OnErrorFetch")
    }

    if ($file.name.EndsWith("ApiTests.cs")){
        $content = $content.Replace(".GetClan", ".FetchClan")
        $content = $content.Replace(".GetCurrent", ".FetchCurrent")
        $content = $content.Replace(".GetPlayer", ".FetchPlayer")
        $content = $content.Replace(".GetLeague", ".FetchLeague")
        $content = $content.Replace(".GetWar", ".FetchWar")
        $content = $content.Replace(".GetLocation", ".FetchLocation")
    }

    if ($file.name -eq "WarClan.cs"){
        $content=$content.Replace($warClanConstructor, "")
    }

    if ($file.name -eq "Clan.cs"){
        $content = $content.Replace($clanConstructor, "")
        $content = $content.Replace($membersProperty, "")
        $content=$content.Replace($memberListHash, "")
        $content=$content.Replace($membersEquals, "")
        $content=$content.Replace('            sb.Append("  MemberList: ").Append(MemberList).Append("\n");
', '')
        $content=$content.Replace($membersConverter, "")
        $content=$content.Replace("            int members = default;
", '')
        $content=$content.Replace("members, ", "")

        $content=$content.Replace("JsonSerializer.Serialize(writer, clan.MemberList, options);", "JsonSerializer.Serialize(writer, clan.Members, options);")
        $content=$content.Replace("            writer.WriteNumber(`"members`", (int)clan.Members);
", "")

        # this is an openapi bug and should not be required
        $content=$content.Replace(
            "return new Clan(badgeUrls, clanLevel, clanPoints, clanVersusPoints, description, isWarLogPublic, labels, memberList, name, requiredTrophies, tag, warLeague, warWinStreak, warWins, chatLanguage, location, type, warFrequency, warLosses, warTies);",
            "return new Clan(badgeUrls, clanLevel, clanPoints, clanVersusPoints, description, isWarLogPublic, labels, memberList, name, requiredTrophies, tag, warLeague, warLosses, warTies, warWinStreak, warWins, chatLanguage, location, type, warFrequency);")
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
        $content = $content.Replace("Sccwl", "SCCWL");
    }

    if ($file.name -eq "ClanWarMember.cs"){
        $content = $content.Replace($mapPosition, "");
    }

    if ($file.name -eq "ClanWar.cs"){
        $content = $content.Replace("public WarClan Clan { get; }", "public WarClan Clan { get; private set; }")
        $content = $content.Replace("public WarClan Opponent { get; }", "public WarClan Opponent { get; private set; }")
        $content = $content.Replace("State = state;", "State = state;`n            Initialize();")
    }

    if (-Not ($originalContent -ceq $content)){
        try {
            Set-Content $file.PSPath $content -ErrorAction Stop
        }
        catch {
            Write-Error "An error occured writing to file $($file.Name)"
            $content = $null
        }
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
