using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RESTAPI.Models.Requests
{
    /// <summary>
    /// Request model for password reset.
    /// </summary>
    public class PasswordResetRequestModel
    {
        [StringLength(256)]
        [RegularExpression(Constants.EMAIL_PATTERN)]
        [JsonPropertyName("emailaddress")]
        public string EmailAddress { get; set; }

        [RegularExpression(Constants.USERNAME_PATTERN, ErrorMessage = "invalid characters in username")]
        [StringLength(64, MinimumLength = 2, ErrorMessage = "invalid username length")]
        [JsonPropertyName("username")]
        public string UserName { get; set; }
    }
}
