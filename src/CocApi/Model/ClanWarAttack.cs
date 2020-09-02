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
    /// ClanWarAttack
    /// </summary>
    [DataContract]
    public partial class ClanWarAttack :  IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWarAttack" /> class.
        /// </summary>
        /// <param name="order">order.</param>
        /// <param name="attackerTag">attackerTag.</param>
        /// <param name="defenderTag">defenderTag.</param>
        /// <param name="stars">stars.</param>
        /// <param name="destructionPercentage">destructionPercentage.</param>
        public ClanWarAttack(int order = default(int), string attackerTag = default(string), string defenderTag = default(string), int stars = default(int), int destructionPercentage = default(int))
        {
            this.Order = order;
            this.AttackerTag = attackerTag;
            this.DefenderTag = defenderTag;
            this.Stars = stars;
            this.DestructionPercentage = destructionPercentage;
        }
        
        /// <summary>
        /// Gets or Sets Order
        /// </summary>
        [DataMember(Name="order", EmitDefaultValue=false)]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or Sets AttackerTag
        /// </summary>
        [DataMember(Name="attackerTag", EmitDefaultValue=false)]
        public string AttackerTag { get; private set; }

        /// <summary>
        /// Gets or Sets DefenderTag
        /// </summary>
        [DataMember(Name="defenderTag", EmitDefaultValue=false)]
        public string DefenderTag { get; private set; }

        /// <summary>
        /// Gets or Sets Stars
        /// </summary>
        [DataMember(Name="stars", EmitDefaultValue=false)]
        public int Stars { get; private set; }

        /// <summary>
        /// Gets or Sets DestructionPercentage
        /// </summary>
        [DataMember(Name="destructionPercentage", EmitDefaultValue=false)]
        public int DestructionPercentage { get; private set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ClanWarAttack {\n");
            sb.Append("  Order: ").Append(Order).Append("\n");
            sb.Append("  AttackerTag: ").Append(AttackerTag).Append("\n");
            sb.Append("  DefenderTag: ").Append(DefenderTag).Append("\n");
            sb.Append("  Stars: ").Append(Stars).Append("\n");
            sb.Append("  DestructionPercentage: ").Append(DestructionPercentage).Append("\n");
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