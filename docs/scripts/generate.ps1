$packageVersion = "2.0.0-preview1.19.1"
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
    'derivedApiPackage=Apis',
    'packageGuid=71B5E000-88E9-432B-BAEB-BB622EA7DC33',
    "dateTimeFormat=yyyyMMdd'T'HHmmss.fff'Z'",
    "dateFormat=yyyy'-'MM"
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
        /// <param name="capitalLeague">capitalLeague</param>
        /// <param name="clanCapital">clanCapital</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="clanPoints">clanPoints</param>
        /// <param name="clanVersusPoints">clanVersusPoints</param>
        /// <param name="description">description</param>
        /// <param name="isFamilyFriendly">isFamilyFriendly</param>
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
        internal Clan(BadgeUrls badgeUrls, CapitalLeague capitalLeague, ClanCapital clanCapital, int clanLevel, int clanPoints, int clanVersusPoints, string description, bool isFamilyFriendly, bool isWarLogPublic, List<Label> labels, List<ClanMember> memberList, int members, string name, int requiredTrophies, string tag, WarLeague warLeague, int warWinStreak, int warWins, Language? chatLanguage = default, Location? location = default, RecruitingType? type = default, WarFrequency? warFrequency = default, int? warLosses = default, int? warTies = default)
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

            if (clanCapital == null)
                throw new ArgumentNullException("clanCapital is a required property for Clan and cannot be null.");

            if (badgeUrls == null)
                throw new ArgumentNullException("badgeUrls is a required property for Clan and cannot be null.");

            if (capitalLeague == null)
                throw new ArgumentNullException("capitalLeague is a required property for Clan and cannot be null.");

            if (isFamilyFriendly == null)
                throw new ArgumentNullException("isFamilyFriendly is a required property for Clan and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            BadgeUrls = badgeUrls;
            CapitalLeague = capitalLeague;
            ClanCapital = clanCapital;
            ClanLevel = clanLevel;
            ClanPoints = clanPoints;
            ClanVersusPoints = clanVersusPoints;
            Description = description;
            IsFamilyFriendly = isFamilyFriendly;
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
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out members);
                            break;

"@

$clanWarMemberConstructor = @"
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWarMember" /> class.
        /// </summary>
        /// <param name="mapPosition">mapPosition</param>
        /// <param name="name">name</param>
        /// <param name="opponentAttacks">opponentAttacks</param>
        /// <param name="tag">tag</param>
        /// <param name="townhallLevel">townhallLevel</param>
        /// <param name="attacks">attacks</param>
        /// <param name="bestOpponentAttack">bestOpponentAttack</param>
        [JsonConstructor]
        internal ClanWarMember(int mapPosition, string name, int opponentAttacks, string tag, int townhallLevel, List<ClanWarAttack>? attacks = default, ClanWarAttack? bestOpponentAttack = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for ClanWarMember and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for ClanWarMember and cannot be null.");

            if (mapPosition == null)
                throw new ArgumentNullException("mapPosition is a required property for ClanWarMember and cannot be null.");

            if (townhallLevel == null)
                throw new ArgumentNullException("townhallLevel is a required property for ClanWarMember and cannot be null.");

            if (opponentAttacks == null)
                throw new ArgumentNullException("opponentAttacks is a required property for ClanWarMember and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            MapPosition = mapPosition;
            Name = name;
            OpponentAttacks = opponentAttacks;
            Tag = tag;
            TownhallLevel = townhallLevel;
            Attacks = attacks;
            BestOpponentAttack = bestOpponentAttack;
        }


"@

$warPreference = @"
    public enum WarPreference
    {
        /// <summary>
        /// Enum Out for value: out
        /// </summary>
        Out = 1,

        /// <summary>
        /// Enum In for value: in
        /// </summary>
        In = 2

    }

"@

$warPreferenceReplacement = @"
    public enum WarPreference
    {
        /// <summary>
        /// Enum Out for value: out
        /// </summary>
        Out = 0,

        /// <summary>
        /// Enum In for value: in
        /// </summary>
        In = 1
    }

"@

$warType = @"
    public enum WarType
    {
        /// <summary>
        /// Enum Unknown for value: unknown
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// Enum Random for value: random
        /// </summary>
        Random = 2,

        /// <summary>
        /// Enum Friendly for value: friendly
        /// </summary>
        Friendly = 3,

        /// <summary>
        /// Enum SCCWL for value: sccwl
        /// </summary>
        SCCWL = 4

    }


"@

$warTypeReplacement = @"
    public enum WarType
    {
        /// <summary>
        /// Enum Unknown for value: unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Enum Random for value: random
        /// </summary>
        Random = 1,

        /// <summary>
        /// Enum Friendly for value: friendly
        /// </summary>
        Friendly = 2,

        /// <summary>
        /// Enum SCCWL for value: sccwl
        /// </summary>
        SCCWL = 3
    }


"@

$clanCapitalRaidSeasonState = @"
        /// <summary>
        /// Defines State
        /// </summary>
        public enum StateEnum
        {
            /// <summary>
            /// Enum Unknown for value: unknown
            /// </summary>
            Unknown = 1,

            /// <summary>
            /// Enum Ongoing for value: ongoing
            /// </summary>
            Ongoing = 2,

            /// <summary>
            /// Enum Ended for value: ended
            /// </summary>
            Ended = 3

        }

"@

$clanCapitalRaidSeasonStateReplacement = @"
        /// <summary>
        /// Defines State
        /// </summary>
        public enum StateEnum
        {
            /// <summary>
            /// Enum Unknown for value: unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Enum Ongoing for value: ongoing
            /// </summary>
            Ongoing = 1,

            /// <summary>
            /// Enum Ended for value: ended
            /// </summary>
            Ended = 2

        }

"@

$apiKey = @"
                    List<TokenBase> tokens = new List<TokenBase>();

                    ApiKeyToken apiKey = (ApiKeyToken) await ApiKeyProvider.GetAsync(cancellationToken).ConfigureAwait(false);

                    tokens.Add(apiKey);


"@

$tokenRateLimit = @"
                        else if (apiResponse.StatusCode == (HttpStatusCode) 429)
                            foreach(TokenBase token in tokens)
                                token.BeginRateLimit();

"@

$groupDeserialize = @"
                        if (apiResponse.IsSuccessStatusCode)
                        {
                            apiResponse.Content = JsonSerializer.Deserialize<ClanWarLeagueGroup>(apiResponse.RawContent, _jsonSerializerOptions);
"@

$groupDeserializeReplacement = @"
                        if (apiResponse.IsSuccessStatusCode && !apiResponse.RawContent.Contains("notInWar"))
                        {
                            apiResponse.Content = JsonSerializer.Deserialize<ClanWarLeagueGroup?>(apiResponse.RawContent, _jsonSerializerOptions);
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
        $content = $content.Replace(".GetCapitalRaidSeasonsAsync", ".FetchCapitalRaidSeasonsAsync")
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

        $content=$content.Replace("JsonSerializer.Serialize(writer, clan.MemberList, jsonSerializerOptions);", "JsonSerializer.Serialize(writer, clan.Members, jsonSerializerOptions);")
        $content=$content.Replace("            writer.WriteNumber(`"members`", clan.Members);
", "")

        # this is an openapi bug and should not be required
        $content=$content.Replace(
            "return new Clan(badgeUrls, capitalLeague, clanCapital, clanLevel, clanPoints, clanVersusPoints, description, isFamilyFriendly, isWarLogPublic, labels, memberList, name, requiredTrophies, tag, warLeague, warWinStreak, warWins, chatLanguage, location, type, warFrequency, warLosses, warTies);",
            "return new Clan(badgeUrls, capitalLeague, clanCapital, clanLevel, clanPoints, clanVersusPoints, description, isFamilyFriendly, isWarLogPublic, labels, memberList, name, requiredTrophies, tag, warLeague, warLosses, warTies, warWinStreak, warWins, chatLanguage, location, type, warFrequency);")
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
        $content = $content.Replace($clanWarMemberConstructor, "")
    }

    if ($file.name -eq "ClanWar.cs"){
        $content = $content.Replace("public WarClan Clan { get; }", "public WarClan Clan { get; private set; }")
        $content = $content.Replace("public WarClan Opponent { get; }", "public WarClan Opponent { get; private set; }")
        $content = $content.Replace("State = state;", "State = state;`n            Initialize();")
        $content = $content.Replace("public int AttacksPerMember { get; }", "public int AttacksPerMember { get; private set; }")
    }

    if ($file.name -eq "ClanCapitalRaidSeason.cs"){
        # TODO: figure out why the index starts at 1
        $content = $content.Replace($clanCapitalRaidSeasonState, $clanCapitalRaidSeasonStateReplacement)
    }

    if ($file.name -eq "WarPreference.cs"){
        $content = $content.Replace($warPreference, $warPreferenceReplacement)
    }

    if ($file.name -eq "WarType.cs"){
        $content = $content.Replace($warType, $warTypeReplacement)
    }

    if ($file.name -eq "DeveloperApi.cs"){
        $content = $content.Replace($apiKey, "")
        $content = $content.Replace($tokenRateLimit, "")
    }

    if ($file.name -eq "ClansApi.cs"){
        $content = $content.Replace($groupDeserialize, $groupDeserializeReplacement)
    }

    if ($file.name -eq "ClanWarLeagueGroup.cs"){
        $content = $content.Replace("public static string SeasonFormat { get; set; } = `"yyyy-MM-dd`";", "public static string SeasonFormat { get; set; } = `"yyyy-MM`";")
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
