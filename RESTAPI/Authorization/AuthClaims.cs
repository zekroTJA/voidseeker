using RESTAPI.Models;
using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Authorization
{
    /// <summary>
    /// Wraps a username and session ID which needs
    /// to be saved with a session key and recovered
    /// by a sesison key.
    /// </summary>
    public class AuthClaims
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("userid")]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }
    }
}
