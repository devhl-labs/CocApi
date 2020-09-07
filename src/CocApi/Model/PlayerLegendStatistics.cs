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

namespace CocApi
{
}



namespace CocApi.Model
{
    /// <summary>
    /// PlayerLegendStatistics
    /// </summary>
    [DataContract]
    public partial class PlayerLegendStatistics :  IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLegendStatistics" /> class.
        /// </summary>
        /// <param name="currentSeason">currentSeason.</param>
        /// <param name="previousVersusSeason">previousVersusSeason.</param>
        /// <param name="bestVersusSeason">bestVersusSeason.</param>
        /// <param name="legendTrophies">legendTrophies.</param>
        /// <param name="previousSeason">previousSeason.</param>
        /// <param name="bestSeason">bestSeason.</param>
        public PlayerLegendStatistics(LegendLeagueTournamentSeasonResult currentSeason = default(LegendLeagueTournamentSeasonResult), LegendLeagueTournamentSeasonResult previousVersusSeason = default(LegendLeagueTournamentSeasonResult), LegendLeagueTournamentSeasonResult bestVersusSeason = default(LegendLeagueTournamentSeasonResult), int legendTrophies = default(int), LegendLeagueTournamentSeasonResult previousSeason = default(LegendLeagueTournamentSeasonResult), LegendLeagueTournamentSeasonResult bestSeason = default(LegendLeagueTournamentSeasonResult))
        {
            this.CurrentSeason = currentSeason;
            this.PreviousVersusSeason = previousVersusSeason;
            this.BestVersusSeason = bestVersusSeason;
            this.LegendTrophies = legendTrophies;
            this.PreviousSeason = previousSeason;
            this.BestSeason = bestSeason;
        }
        
        /// <summary>
        /// Gets or Sets CurrentSeason
        /// </summary>
        [DataMember(Name="currentSeason", EmitDefaultValue=false)]
        public LegendLeagueTournamentSeasonResult CurrentSeason { get; private set; }

        ///// <summary>
        ///// Gets or Sets PreviousVersusSeason
        ///// </summary>
        //[DataMember(Name="previousVersusSeason", EmitDefaultValue=false)]
        //public LegendLeagueTournamentSeasonResult PreviousVersusSeason { get; private set; }

        ///// <summary>
        ///// Gets or Sets BestVersusSeason
        ///// </summary>
        //[DataMember(Name="bestVersusSeason", EmitDefaultValue=false)]
        //public LegendLeagueTournamentSeasonResult BestVersusSeason { get; private set; }

        ///// <summary>
        ///// Gets or Sets LegendTrophies
        ///// </summary>
        //[DataMember(Name="legendTrophies", EmitDefaultValue=false)]
        //public int LegendTrophies { get; private set; }

        ///// <summary>
        ///// Gets or Sets PreviousSeason
        ///// </summary>
        //[DataMember(Name="previousSeason", EmitDefaultValue=false)]
        //public LegendLeagueTournamentSeasonResult PreviousSeason { get; private set; }

        ///// <summary>
        ///// Gets or Sets BestSeason
        ///// </summary>
        //[DataMember(Name="bestSeason", EmitDefaultValue=false)]
        //public LegendLeagueTournamentSeasonResult BestSeason { get; private set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PlayerLegendStatistics {\n");
            sb.Append("  CurrentSeason: ").Append(CurrentSeason).Append("\n");
            sb.Append("  PreviousVersusSeason: ").Append(PreviousVersusSeason).Append("\n");
            sb.Append("  BestVersusSeason: ").Append(BestVersusSeason).Append("\n");
            sb.Append("  LegendTrophies: ").Append(LegendTrophies).Append("\n");
            sb.Append("  PreviousSeason: ").Append(PreviousSeason).Append("\n");
            sb.Append("  BestSeason: ").Append(BestSeason).Append("\n");
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