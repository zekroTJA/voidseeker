using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    /// <summary>
    /// Response model if instance information.
    /// </summary>
    public class InstanceStatusModel
    {
        [JsonPropertyName("initialized")]
        public bool Initialized { get; set; }

        [JsonPropertyName("userscount")]
        public long UsersCount { get; set; }

        [JsonPropertyName("imagescount")]
        public long ImagesCount { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
