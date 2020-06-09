using RESTAPI.Models;
using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Authorization
{
    /// <summary>
    /// Wraps a UserName, UserUid and the fully
    /// hydrated user model of an authenticted
    /// user.
    /// </summary>
    public class AuthClaims
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("userid")]
        public Guid UserUid { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }
    }
}
