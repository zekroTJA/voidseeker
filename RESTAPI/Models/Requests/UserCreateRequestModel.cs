using RESTAPI.Extensions;
using System.Text.Json.Serialization;

namespace RESTAPI.Models.Requests
{
    public class UserCreateRequestModel : UserModel
    {
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("oldpassword")]
        public string? OldPassword { get; set; }

        [JsonPropertyName("emailaddress")]
        public new string EmailAddress { get; set; }

        public bool Verify() => 
            !( UserName.NullOrEmpty() 
            || Password.NullOrEmpty() );
    }
}
