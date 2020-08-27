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
    /// PlayerItemLevel
    /// </summary>
    [DataContract]
    public partial class PlayerItemLevel :  IEquatable<PlayerItemLevel>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerItemLevel" /> class.
        /// </summary>
        /// <param name="level">level.</param>
        /// <param name="name">name.</param>
        /// <param name="maxLevel">maxLevel.</param>
        /// <param name="village">village.</param>
        public PlayerItemLevel(int level = default(int), Object name = default(Object), int maxLevel = default(int), string village = default(string))
        {
            this.Level = level;
            this.Name = name;
            this.MaxLevel = maxLevel;
            this.Village = village;
        }
        
        /// <summary>
        /// Gets or Sets Level
        /// </summary>
        [DataMember(Name="level", EmitDefaultValue=false)]
        public int Level { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public Object Name { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets MaxLevel
        /// </summary>
        [DataMember(Name="maxLevel", EmitDefaultValue=false)]
        public int MaxLevel { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Village
        /// </summary>
        [DataMember(Name="village", EmitDefaultValue=false)]
        public string Village { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PlayerItemLevel {\n");
            sb.Append("  Level: ").Append(Level).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  MaxLevel: ").Append(MaxLevel).Append("\n");
            sb.Append("  Village: ").Append(Village).Append("\n");
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
            return this.Equals(input as PlayerItemLevel);
        }

        /// <summary>
        /// Returns true if PlayerItemLevel instances are equal
        /// </summary>
        /// <param name="input">Instance of PlayerItemLevel to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PlayerItemLevel input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Level == input.Level ||
                    this.Level.Equals(input.Level)
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.MaxLevel == input.MaxLevel ||
                    this.MaxLevel.Equals(input.MaxLevel)
                ) && 
                (
                    this.Village == input.Village ||
                    (this.Village != null &&
                    this.Village.Equals(input.Village))
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
