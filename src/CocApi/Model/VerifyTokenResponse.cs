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
    /// VerifyTokenResponse
    /// </summary>
    [DataContract]
    public partial class VerifyTokenResponse :  IEquatable<VerifyTokenResponse>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyTokenResponse" /> class.
        /// </summary>
        /// <param name="tag">tag.</param>
        /// <param name="token">token.</param>
        /// <param name="status">status.</param>
        public VerifyTokenResponse(string tag = default(string), string token = default(string), string status = default(string))
        {
            this.Tag = tag;
            this.Token = token;
            this.Status = status;
        }
        
        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [DataMember(Name="tag", EmitDefaultValue=false)]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or Sets Token
        /// </summary>
        [DataMember(Name="token", EmitDefaultValue=false)]
        public string Token { get; set; }

        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name="status", EmitDefaultValue=false)]
        public string Status { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class VerifyTokenResponse {\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  Token: ").Append(Token).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
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
            return this.Equals(input as VerifyTokenResponse);
        }

        /// <summary>
        /// Returns true if VerifyTokenResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of VerifyTokenResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(VerifyTokenResponse input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Tag == input.Tag ||
                    (this.Tag != null &&
                    this.Tag.Equals(input.Tag))
                ) && 
                (
                    this.Token == input.Token ||
                    (this.Token != null &&
                    this.Token.Equals(input.Token))
                ) && 
                (
                    this.Status == input.Status ||
                    (this.Status != null &&
                    this.Status.Equals(input.Status))
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
