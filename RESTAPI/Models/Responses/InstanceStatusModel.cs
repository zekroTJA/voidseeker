using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    public class InstanceStatusModel
    {
        [JsonPropertyName("initialized")]
        public bool Initialized { get; set; }

        [JsonPropertyName("userscount")]
        public long UsersCount { get; set; }
    }
}
