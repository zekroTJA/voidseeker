using System.Text.Json.Serialization;

namespace RESTAPI.Models.Requests
{
    public class LoginModel
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("remember")]
        public bool Remember { get; set; }
    }
}
