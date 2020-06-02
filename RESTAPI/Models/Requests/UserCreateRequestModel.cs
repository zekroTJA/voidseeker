using RESTAPI.Extensions;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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

        public bool ValidateUsername() =>
            Regex.IsMatch(UserName, @"^[a-z0-9-_\.]{1,32}$");

        public bool ValidatePassword() =>
            !Password.NullOrEmpty();
    }
}
