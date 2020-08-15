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
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = CocApi.Client.OpenAPIDateConverter;

namespace CocApi.Model
{
/// <summary>
    /// ClanWarAttack
    /// </summary>
    [DataContract]
    public partial class ClanWarAttack :  IEquatable<ClanWarAttack>, IValidatableObject
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
        public int Order { get; set; }

        /// <summary>
        /// Gets or Sets AttackerTag
        /// </summary>
        [DataMember(Name="attackerTag", EmitDefaultValue=false)]
        public string AttackerTag { get; set; }

        /// <summary>
        /// Gets or Sets DefenderTag
        /// </summary>
        [DataMember(Name="defenderTag", EmitDefaultValue=false)]
        public string DefenderTag { get; set; }

        /// <summary>
        /// Gets or Sets Stars
        /// </summary>
        [DataMember(Name="stars", EmitDefaultValue=false)]
        public int Stars { get; set; }

        /// <summary>
        /// Gets or Sets DestructionPercentage
        /// </summary>
        [DataMember(Name="destructionPercentage", EmitDefaultValue=false)]
        public int DestructionPercentage { get; set; }

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
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as ClanWarAttack);
        }

        /// <summary>
        /// Returns true if ClanWarAttack instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanWarAttack to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanWarAttack input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Order == input.Order ||
                    this.Order.Equals(input.Order)
                ) && 
                (
                    this.AttackerTag == input.AttackerTag ||
                    (this.AttackerTag != null &&
                    this.AttackerTag.Equals(input.AttackerTag))
                ) && 
                (
                    this.DefenderTag == input.DefenderTag ||
                    (this.DefenderTag != null &&
                    this.DefenderTag.Equals(input.DefenderTag))
                ) && 
                (
                    this.Stars == input.Stars ||
                    this.Stars.Equals(input.Stars)
                ) && 
                (
                    this.DestructionPercentage == input.DestructionPercentage ||
                    this.DestructionPercentage.Equals(input.DestructionPercentage)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                return hashCode;
            }
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
