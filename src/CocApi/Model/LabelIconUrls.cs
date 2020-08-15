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
    /// LabelIconUrls
    /// </summary>
    [DataContract]
    public partial class LabelIconUrls :  IEquatable<LabelIconUrls>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelIconUrls" /> class.
        /// </summary>
        /// <param name="tiny">tiny.</param>
        /// <param name="small">small.</param>
        /// <param name="medium">medium.</param>
        public LabelIconUrls(string tiny = default(string), string small = default(string), string medium = default(string))
        {
            this.Tiny = tiny;
            this.Small = small;
            this.Medium = medium;
        }
        
        /// <summary>
        /// Gets or Sets Tiny
        /// </summary>
        [DataMember(Name="tiny", EmitDefaultValue=false)]
        public string Tiny { get; set; }

        /// <summary>
        /// Gets or Sets Small
        /// </summary>
        [DataMember(Name="small", EmitDefaultValue=false)]
        public string Small { get; set; }

        /// <summary>
        /// Gets or Sets Medium
        /// </summary>
        [DataMember(Name="medium", EmitDefaultValue=false)]
        public string Medium { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class LabelIconUrls {\n");
            sb.Append("  Tiny: ").Append(Tiny).Append("\n");
            sb.Append("  Small: ").Append(Small).Append("\n");
            sb.Append("  Medium: ").Append(Medium).Append("\n");
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
            return this.Equals(input as LabelIconUrls);
        }

        /// <summary>
        /// Returns true if LabelIconUrls instances are equal
        /// </summary>
        /// <param name="input">Instance of LabelIconUrls to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LabelIconUrls input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Tiny == input.Tiny ||
                    (this.Tiny != null &&
                    this.Tiny.Equals(input.Tiny))
                ) && 
                (
                    this.Small == input.Small ||
                    (this.Small != null &&
                    this.Small.Equals(input.Small))
                ) && 
                (
                    this.Medium == input.Medium ||
                    (this.Medium != null &&
                    this.Medium.Equals(input.Medium))
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
