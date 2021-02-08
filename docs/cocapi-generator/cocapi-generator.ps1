$ErrorActionPreference = "Stop"

$root="$PSScriptRoot/../.."

Move-Item -Path "$root/src/CocApi/Additions" -Destination "$root/.."

Remove-Item -Path "$root/src/CocApi" -Recurse

cmd /c start /wait java -jar ..\..\..\openapi-generator\modules\openapi-generator-cli\target\openapi-generator-cli.jar generate `
    -g csharp-netcore `
    -i ..\..\..\Clash-of-Clans-Swagger\swagger.yml `
    -c generator-config.json `
    -o ..\..\src\CocApi `
    -t templates `
    --library httpclient `
    --global-property apiTests,modelTests | Out-Null

function Get-ContentWithRename {
    [CmdletBinding()]
    param (
        [string]
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $content,
  
        [string]
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $searchText, 

        [string]
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $replacementText
    )
  
      $searchPhrase=
"                throw new ArgumentNullException(nameof($searchText)); 

            clanTag = Clash.FormatTag(clanTag);"

    $replacementPhrase="                throw new ArgumentNullException(nameof($searchText)); `n`n            $replacementText = Clash.FormatTag($replacementText);"
  
      Write-Verbose $replacementPhrase
  
      $content.Replace($searchPhrase, $replacementPhrase)
}

function Get-ContentWithoutClanTag {
    [CmdletBinding()]
    param (
        [string]
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $content,
  
        [string]
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $searchText
    )
  
      $searchPhrase=
"                throw new ArgumentNullException(nameof($searchText)); 

            clanTag = Clash.FormatTag(clanTag);"

    $replacementPhrase="                throw new ArgumentNullException(nameof($searchText));"

      $content.Replace($searchPhrase, $replacementPhrase)
}

function Get-ContentWithoutLeadingFormatTag {
    [CmdletBinding()]
    param (
        [string]
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $content
    )
      $content.Replace(
'        {

            clanTag = Clash.FormatTag(clanTag);',
'        {')
}
  

$files = Get-ChildItem ..\..\src\CocApi -Recurse
foreach ($file in $files)
{    
    if ($file.PSIsContainer){
        continue
    }

    $content=Get-Content $file.PSPath -raw

    if (-Not($content)){
        continue;
    }

    $content=$content -replace '\?{3,4}', '?' # replace every three to four consecutive occurrences of '?' with a single one
    $content=$content.Replace("WithHttpInfoAsync(", "ResponseAsync(")

    if ($file.name.EndsWith("Api.cs")){
        $content=$content.Replace('> Get', '> Fetch')
        $content=$content.Replace('?Get', 'Fetch')
        $content=$content.Replace('?Search', 'Search')
        $content=$content.Replace('?Verify', 'Verify')
    }

    if ($file.name -eq "ClansApi.cs"){
        $content=$content.Replace('public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> FetchCurrentWarResponseAsync(string clanTag, System.Threading.CancellationToken? cancellationToken = null)', 'internal async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> InternalFetchCurrentWarResponseAsync(string clanTag, System.Threading.CancellationToken? cancellationToken = null)')
        $content=$content.Replace('public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> FetchClanWarLeagueWarResponseAsync(string warTag, System.Threading.CancellationToken? cancellationToken = null)', 'internal async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> InternalFetchClanWarLeagueWarResponseAsync(string warTag, System.Threading.CancellationToken? cancellationToken = null)')
        $content=Get-ContentWithRename -content $content -searchText 'warTag' -replacementText 'warTag' 
        $content=Get-ContentWithoutLeadingFormatTag -content $content
    }

    if ($file.name -eq "LabelsApi.cs"){
        $content=Get-ContentWithoutLeadingFormatTag -content $content
    }

    if ($file.name -eq "LeaguesApi.cs"){
        $content=Get-ContentWithoutClanTag -content $content -searchText 'leagueId'
        $content=Get-ContentWithoutClanTag -content $content -searchText 'seasonId'
        $content=Get-ContentWithoutLeadingFormatTag -content $content
    }

    if ($file.name -eq "LocationsApi.cs"){
        $content=Get-ContentWithoutClanTag -content $content -searchText 'locationId'
        $content=Get-ContentWithoutLeadingFormatTag -content $content
    }

    if ($file.name -eq "PlayersApi.cs"){
        $content=Get-ContentWithRename -content $content -searchText 'playerTag' -replacementText 'playerTag'  
        $content=Get-ContentWithRename -content $content -searchText 'body' -replacementText 'playerTag'            
    }

    if ($file.name -eq "Clan.cs"){
        $content=$content.Replace('public Clan(WarLeague warLeague, List<ClanMember> memberList, int requiredTrophies, int clanVersusPoints, string tag, bool isWarLogPublic, WarFrequencyEnum? warFrequency, int clanLevel, int warWinStreak, int warWins, int warTies, int warLosses, int clanPoints, List<Label> labels, string name, Location location, TypeEnum? type, int members, string description, ClanBadgeUrls badgeUrls)', 'public Clan(WarLeague warLeague = default(WarLeague), List<ClanMember> memberList = default(List<ClanMember>), int requiredTrophies = default(int), int clanVersusPoints = default(int), string tag = default(string), bool isWarLogPublic = default(bool), WarFrequency? warFrequency = default(WarFrequency?), int clanLevel = default(int), int warWinStreak = default(int), int warWins = default(int), int warTies = default(int), int warLosses = default(int), int clanPoints = default(int), List<Label> labels = default(List<Label>), string name = default(string), Location location = default(Location), RecruitingType? type = default(RecruitingType?), /*int members = default(int),*/ string description = default(string), ClanBadgeUrls badgeUrls = default(ClanBadgeUrls))')
        $content=$content.Replace('public TypeEnum? Type { get; private set; }', 'public RecruitingType? Type { get; private set; }')
        $content=$content.Replace('public WarFrequencyEnum? WarFrequency { get; private set; }', 'public WarFrequency? WarFrequency { get; private set; }')
        $content=$content.Replace('public Location Location { get; private set; }', 'public Location? Location { get; private set; }')
        $content=$content.Replace('[DataMember(Name = "members", EmitDefaultValue = false)]', '[DataMember(Name = "memberList", EmitDefaultValue = false)]')
        $content=$content.Replace('public int Members { get; private set; }', 'public List<ClanMember> Members { get; private set; }')
        $content=$content.Replace('MemberList = memberList;', '//MemberList = memberList;')
        $content=$content.Replace('Members = members;', 'Members = memberList;')
        $content=$content.Replace(
'                (
                    MemberList == input.MemberList ||
                    MemberList != null &&
                    input.MemberList != null &&
                    MemberList.SequenceEqual(input.MemberList)
                ) && ',
'')
        $content=$content.Replace(
'            sb.Append("  MemberList: ").Append(MemberList).Append(''\n'');',
'')
        $content=$content.Replace(
'        /// <summary>
        /// Gets or Sets MemberList
        /// </summary>
        [DataMember(Name = "memberList", EmitDefaultValue = false)]
        public List<ClanMember> MemberList { get; private set; }'
,''
        )
    }

    if ($file.name -eq "ClanMember.cs"){
        $content=$content.Replace('public RoleEnum? Role { get; private set; }', 'public Role? Role { get; private set; }')
        $content=$content.Replace('public ClanMember(League league, string tag, string name, RoleEnum? role, int expLevel, int clanRank, int previousClanRank, int donations, int donationsReceived, int trophies, int versusTrophies)', 'public ClanMember(League league = default(League), string tag = default(string), string name = default(string), Role? role = default(Role?), int expLevel = default(int), int clanRank = default(int), int previousClanRank = default(int), int donations = default(int), int donationsReceived = default(int), int trophies = default(int), int versusTrophies = default(int))')
    }

    if ($file.name -eq "ClanWar.cs"){
        $content=$content.Replace('public ClanWar(WarClan clan, int teamSize, WarClan opponent, DateTime startTime, StateEnum? state, DateTime endTime, DateTime preparationStartTime)', 'public ClanWar(List<ClanWarAttack> allAttacks, WarClan clan = default(WarClan), int teamSize = default(int), WarClan opponent = default(WarClan), DateTime startTime = default(DateTime), WarState? state = default(WarState?), DateTime endTime = default(DateTime), DateTime preparationStartTime = default(DateTime))')
        $content=$content.Replace('public StateEnum? State { get; private set; }', 'public WarState? State { get; private set; }')
    }

    if ($file.name -eq "ClanWarLeagueGroup.cs"){
        $content=$content.Replace('public ClanWarLeagueGroup(string tag, StateEnum? state, DateTime season, List<ClanWarLeagueClan> clans, List<ClanWarLeagueRound> rounds)', 'public ClanWarLeagueGroup(string tag = default(string), GroupState? state = default(GroupState?), DateTime season = default(DateTime), List<ClanWarLeagueClan> clans = default(List<ClanWarLeagueClan>), List<ClanWarLeagueRound> rounds = default(List<ClanWarLeagueRound>))')
        $content=$content.Replace('public StateEnum? State { get; private set; }', 'public GroupState? State { get; private set; }')
    }

    if ($file.name -eq "ClanWarMember.cs"){
        $content=$content.Replace('public ClanWarMember(string tag, string name, int mapPosition, int townhallLevel, int opponentAttacks, ClanWarAttack bestOpponentAttack, List<ClanWarAttack> attacks)', 'public ClanWarMember(int mapPositionCorrected = default(int), string tag = default(string), string name = default(string), int mapPosition = default(int), int townhallLevel = default(int), int opponentAttacks = default(int), ClanWarAttack bestOpponentAttack = default(ClanWarAttack), List<ClanWarAttack> attacks = default(List<ClanWarAttack>))')
        $content=$content.Replace('[DataMember(Name = "mapPosition", EmitDefaultValue = false)]', '[DataMember(Name = "mapPositionCorrected", EmitDefaultValue = false)]')
        $content=$content.Replace('public int MapPosition { get; private set; }', 'public int MapPosition { get; internal set; }')
        $content=$content.Replace('public List<ClanWarAttack> Attacks { get; private set; }', 'public List<ClanWarAttack>? Attacks { get; private set; }')  
        $content=$content.Replace('MapPosition = mapPosition;', "RosterPosition = mapPosition;`n            MapPosition = mapPositionCorrected;")      
    }

    if ($file.name -eq "ClanWarLeagueGroup.cs"){
        $content=$content.Replace('public ClanWarLeagueGroup(string tag = default(string), GroupState? state = default(GroupState?), DateTime season = default(DateTime), List<ClanWarLeagueClan> clans = default(List<ClanWarLeagueClan>), List<ClanWarLeagueRound> rounds = default(List<ClanWarLeagueRound>))', 'public ClanWarLeagueGroup(string tag = default(string), GroupState? state = default(GroupState?), DateTime season = default(DateTime), List<ClanWarLeagueClan> clans = default(List<ClanWarLeagueClan>), List<ClanWarLeagueRound> rounds = default(List<ClanWarLeagueRound>))')
    }

    if ($file.name -eq "Player.cs"){
        $content=$content.Replace('public Player(PlayerClan clan, League league, RoleEnum? role, int attackWins, int defenseWins, int townHallLevel, int townHallWeaponLevel, int versusBattleWins, PlayerLegendStatistics legendStatistics, List<PlayerItemLevel> troops, List<PlayerItemLevel> heroes, List<PlayerItemLevel> spells, List<Label> labels, string tag, string name, int expLevel, int trophies, int bestTrophies, int donations, int donationsReceived, int builderHallLevel, int versusTrophies, int bestVersusTrophies, int warStars, List<PlayerAchievementProgress> achievements, int versusBattleWinCount)', 'public Player(PlayerClan clan, League league, Role? role, int attackWins, int defenseWins, int townHallLevel, int townHallWeaponLevel, int versusBattleWins, PlayerLegendStatistics legendStatistics, List<PlayerItemLevel> troops, List<PlayerItemLevel> heroes, List<PlayerItemLevel> spells, List<Label> labels, string tag, string name, int expLevel, int trophies, int bestTrophies, int donations, int donationsReceived, int builderHallLevel, int versusTrophies, int bestVersusTrophies, int warStars, List<PlayerAchievementProgress> achievements, int versusBattleWinCount)')
        $content=$content.Replace('public PlayerClan Clan { get; private set; }', 'public PlayerClan? Clan { get; private set; }')          
        $content=$content.Replace('public League League { get; private set; }', 'public League? League { get; private set; }')
        $content=$content.Replace('public RoleEnum? Role { get; private set; }', 'public Role? Role { get; private set; }') 
    }

    if ($file.name -eq "PlayerLegendStatistics.cs"){
        $content=$content.Replace('public LegendLeagueTournamentSeasonResult PreviousVersusSeason { get; private set; }', 'public LegendLeagueTournamentSeasonResult? PreviousVersusSeason { get; private set; }')
        $content=$content.Replace('public LegendLeagueTournamentSeasonResult BestVersusSeason { get; private set; }', 'public LegendLeagueTournamentSeasonResult? BestVersusSeason { get; private set; }')
        $content=$content.Replace('public LegendLeagueTournamentSeasonResult PreviousSeason { get; private set; }', 'public LegendLeagueTournamentSeasonResult? PreviousSeason { get; private set; }')
        $content=$content.Replace('public LegendLeagueTournamentSeasonResult BestSeason { get; private set; }', 'public LegendLeagueTournamentSeasonResult? BestSeason { get; private set; }')
    }

    Set-Content $file.PSPath $content    
}

Remove-Item -Path "$root/src/CocApi/.gitignore"
Remove-Item -Path "$root/src/CocApi/CocApi.sln"
Remove-Item -Path "$root/src/CocApi/git_push.sh"
Move-Item -Path "$root/src/CocApi/src/CocApi/Api" -Destination "$root/src/CocApi/Api"
Move-Item -Path "$root/src/CocApi/src/CocApi/Client" -Destination "$root/src/CocApi/Client"
Move-Item -Path "$root/src/CocApi/src/CocApi/Model" -Destination "$root/src/CocApi/Model"
Move-Item -Path "$root/src/CocApi/src/CocApi/CocApi.csproj" -Destination "$root/src/CocApi"
Remove-Item -Path "$root/src/CocApi/src" -Recurse
Move-Item -Path "$root/../Additions" -Destination "$root/src/CocApi"