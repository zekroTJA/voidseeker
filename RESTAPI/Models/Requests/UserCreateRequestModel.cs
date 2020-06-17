using RESTAPI.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RESTAPI.Models.Requests
{
    /// <summary>
    /// Request body model used for creating and
    /// updating users.
    /// </summary>
    public class UserCreateRequestModel : UserModel
    {
        [StringLength(4096, MinimumLength = 6)]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("oldpassword")]
        public string? OldPassword { get; set; }

        /// <summary>
        /// Returns true if the username
        /// set is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValidUsername() =>
            Regex.IsMatch(UserName, Constants.USERNAME_PATTERN);

        /// <summary>
        /// Returns true if the set password
        /// is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValidPassword() =>
            !Password.IsNullOrEmpty();
    }
}
