using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RESTAPI.Models.Requests
{
    /// <summary>
    /// The request model to confirm a passowrd
    /// reset wrapping token and new password.
    /// </summary>
    public class PasswordConfirmRequestModel
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [StringLength(4096, MinimumLength = 6)]
        [JsonPropertyName("newpassword")]
        public string NewPassword { get; set; }
    }
}
