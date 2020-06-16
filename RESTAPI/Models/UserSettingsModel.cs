using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public class UserSettingsModel
    {
        [JsonPropertyName("tagblacklist")]
        public string[] TagBlacklist { get; set; }
    }
}
