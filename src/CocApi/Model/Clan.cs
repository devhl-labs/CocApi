/* 
 * Clash of Clans API
 *
 * Check out <a href=\"https://developer.clashofclans.com/#/getting-started\" target=\"_parent\">Getting Started</a> for instructions and links to other resources. Clash of Clans API uses <a href=\"https://jwt.io/\" target=\"_blank\">JSON Web Tokens</a> for authorizing the requests. Tokens are created by developers on <a href=\"https://developer.clashofclans.com/#/account\" target=\"_parent\">My Account</a> page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \"Authorization: Bearer API_TOKEN\". 
 *
 * The version of the OpenAPI document: v1
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = CocApi.Client.OpenAPIDateConverter;

//namespace CocApi
//{
//        /// <summary>
//        /// Defines WarFrequency
//        /// </summary>
//        [JsonConverter(typeof(StringEnumConverter))]
//        public enum WarFrequencyEnum
//        {
//            /// <summary>
//            /// Enum Unknown for value: unknown
//            /// </summary>
//            [EnumMember(Value = "unknown")]
//            Unknown = 1,

//            /// <summary>
//            /// Enum Never for value: never
//            /// </summary>
//            [EnumMember(Value = "never")]
//            Never = 2,

//            /// <summary>
//            /// Enum LessThanOncePerWeek for value: lessThanOncePerWeek
//            /// </summary>
//            [EnumMember(Value = "lessThanOncePerWeek")]
//            LessThanOncePerWeek = 3,

//            /// <summary>
//            /// Enum OncePerWeek for value: oncePerWeek
//            /// </summary>
//            [EnumMember(Value = "oncePerWeek")]
//            OncePerWeek = 4,

//            /// <summary>
//            /// Enum MoreThanOncePerWeek for value: moreThanOncePerWeek
//            /// </summary>
//            [EnumMember(Value = "moreThanOncePerWeek")]
//            MoreThanOncePerWeek = 5,

//            /// <summary>
//            /// Enum Always for value: always
//            /// </summary>
//            [EnumMember(Value = "always")]
//            Always = 6

//        }

//        /// <summary>
//        /// Defines Type
//        /// </summary>
//        [JsonConverter(typeof(StringEnumConverter))]
//        public enum TypeEnum
//        {
//            /// <summary>
//            /// Enum InviteOnly for value: InviteOnly
//            /// </summary>
//            [EnumMember(Value = "InviteOnly")]
//            InviteOnly = 1,

//            /// <summary>
//            /// Enum Closed for value: Closed
//            /// </summary>
//            [EnumMember(Value = "Closed")]
//            Closed = 2,

//            /// <summary>
//            /// Enum Open for value: Open
//            /// </summary>
//            [EnumMember(Value = "Open")]
//            Open = 3

//        }

//}



namespace CocApi.Model
{
    /// <summary>
    /// Clan
    /// </summary>
    [DataContract]
    public partial class Clan :  IValidatableObject 
    {
        ///// <summary>
        ///// Gets or Sets WarFrequency
        ///// </summary>
        //[DataMember(Name="warFrequency", EmitDefaultValue=false)]
        //public WarFrequencyEnum? WarFrequency { get; private set; }

        ///// <summary>
        ///// Gets or Sets Type
        ///// </summary>
        //[DataMember(Name="type", EmitDefaultValue=false)]
        //public TypeEnum? Type { get; private set; }


        ///// <summary>
        ///// Initializes a new instance of the <see cref="Clan" /> class.
        ///// </summary>
        ///// <param name="warLeague">warLeague.</param>
        ///// <param name="memberList">memberList.</param>
        ///// <param name="requiredTrophies">requiredTrophies.</param>
        ///// <param name="clanVersusPoints">clanVersusPoints.</param>
        ///// <param name="tag">tag.</param>
        ///// <param name="isWarLogPublic">isWarLogPublic.</param>
        ///// <param name="warFrequency">warFrequency.</param>
        ///// <param name="clanLevel">clanLevel.</param>
        ///// <param name="warWinStreak">warWinStreak.</param>
        ///// <param name="warWins">warWins.</param>
        ///// <param name="warTies">warTies.</param>
        ///// <param name="warLosses">warLosses.</param>
        ///// <param name="clanPoints">clanPoints.</param>
        ///// <param name="labels">labels.</param>
        ///// <param name="name">name.</param>
        ///// <param name="location">location.</param>
        ///// <param name="type">type.</param>
        ///// <param name="members">members.</param>
        ///// <param name="description">description.</param>
        ///// <param name="badgeUrls">badgeUrls.</param>
        //public Clan(WarLeague warLeague = default(WarLeague), List<ClanMember> memberList = default(List<ClanMember>), int requiredTrophies = default(int), int clanVersusPoints = default(int), string tag = default(string), bool isWarLogPublic = default(bool), WarFrequencyEnum? warFrequency = default(WarFrequencyEnum?), int clanLevel = default(int), int warWinStreak = default(int), int warWins = default(int), int warTies = default(int), int warLosses = default(int), int clanPoints = default(int), List<Label> labels = default(List<Label>), string name = default(string), Location location = default(Location), TypeEnum? type = default(TypeEnum?), int members = default(int), string description = default(string), ClanBadgeUrls badgeUrls = default(ClanBadgeUrls))
        //{
        //    this.WarLeague = warLeague;
        //    this.MemberList = memberList;
        //    this.RequiredTrophies = requiredTrophies;
        //    this.ClanVersusPoints = clanVersusPoints;
        //    this.Tag = tag;
        //    this.IsWarLogPublic = isWarLogPublic;
        //    this.WarFrequency = warFrequency;
        //    this.ClanLevel = clanLevel;
        //    this.WarWinStreak = warWinStreak;
        //    this.WarWins = warWins;
        //    this.WarTies = warTies;
        //    this.WarLosses = warLosses;
        //    this.ClanPoints = clanPoints;
        //    this.Labels = labels;
        //    this.Name = name;
        //    this.Location = location;
        //    this.Type = type;
        //    this.Members = members;
        //    this.Description = description;
        //    this.BadgeUrls = badgeUrls;
        //}

        /// <summary>
        /// Gets or Sets WarLeague
        /// </summary>
        [DataMember(Name = "warLeague", EmitDefaultValue = false)]
        public WarLeague WarLeague { get; private set; }

        ///// <summary>
        ///// Gets or Sets MemberList
        ///// </summary>
        //[DataMember(Name="memberList", EmitDefaultValue=false)]
        //public List<ClanMember> MemberList { get; private set; }

        /// <summary>
        /// Gets or Sets RequiredTrophies
        /// </summary>
        [DataMember(Name="requiredTrophies", EmitDefaultValue=false)]
        public int RequiredTrophies { get; private set; }

        /// <summary>
        /// Gets or Sets ClanVersusPoints
        /// </summary>
        [DataMember(Name="clanVersusPoints", EmitDefaultValue=false)]
        public int ClanVersusPoints { get; private set; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [DataMember(Name="tag", EmitDefaultValue=false)]
        public string Tag { get; private set; }

        /// <summary>
        /// Gets or Sets IsWarLogPublic
        /// </summary>
        [DataMember(Name="isWarLogPublic", EmitDefaultValue=false)]
        public bool IsWarLogPublic { get; private set; }

        /// <summary>
        /// Gets or Sets ClanLevel
        /// </summary>
        [DataMember(Name="clanLevel", EmitDefaultValue=false)]
        public int ClanLevel { get; private set; }

        /// <summary>
        /// Gets or Sets WarWinStreak
        /// </summary>
        [DataMember(Name="warWinStreak", EmitDefaultValue=false)]
        public int WarWinStreak { get; private set; }

        /// <summary>
        /// Gets or Sets WarWins
        /// </summary>
        [DataMember(Name="warWins", EmitDefaultValue=false)]
        public int WarWins { get; private set; }

        /// <summary>
        /// Gets or Sets WarTies
        /// </summary>
        [DataMember(Name="warTies", EmitDefaultValue=false)]
        public int WarTies { get; private set; }

        /// <summary>
        /// Gets or Sets WarLosses
        /// </summary>
        [DataMember(Name="warLosses", EmitDefaultValue=false)]
        public int WarLosses { get; private set; }

        /// <summary>
        /// Gets or Sets ClanPoints
        /// </summary>
        [DataMember(Name="clanPoints", EmitDefaultValue=false)]
        public int ClanPoints { get; private set; }

        /// <summary>
        /// Gets or Sets Labels
        /// </summary>
        [DataMember(Name="labels", EmitDefaultValue=false)]
        public List<Label> Labels { get; private set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; private set; }

        ///// <summary>
        ///// Gets or Sets Location
        ///// </summary>
        //[DataMember(Name="location", EmitDefaultValue=false)]
        //public Location Location { get; private set; }

        ///// <summary>
        ///// Gets or Sets Members
        ///// </summary>
        //[DataMember(Name="members", EmitDefaultValue=false)]
        //public int Members { get; private set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name="description", EmitDefaultValue=false)]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or Sets BadgeUrls
        /// </summary>
        [DataMember(Name="badgeUrls", EmitDefaultValue=false)]
        public ClanBadgeUrls BadgeUrls { get; private set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Clan {\n");
            sb.Append("  WarLeague: ").Append(WarLeague).Append("\n");
            //sb.Append("  MemberList: ").Append(MemberList).Append("\n");
            sb.Append("  RequiredTrophies: ").Append(RequiredTrophies).Append("\n");
            sb.Append("  ClanVersusPoints: ").Append(ClanVersusPoints).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  IsWarLogPublic: ").Append(IsWarLogPublic).Append("\n");
            sb.Append("  WarFrequency: ").Append(WarFrequency).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
            sb.Append("  WarWinStreak: ").Append(WarWinStreak).Append("\n");
            sb.Append("  WarWins: ").Append(WarWins).Append("\n");
            sb.Append("  WarTies: ").Append(WarTies).Append("\n");
            sb.Append("  WarLosses: ").Append(WarLosses).Append("\n");
            sb.Append("  ClanPoints: ").Append(ClanPoints).Append("\n");
            sb.Append("  Labels: ").Append(Labels).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Location: ").Append(Location).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Members: ").Append(Members).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}