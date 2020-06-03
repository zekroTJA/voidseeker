using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public class TagModel : UniqueModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creatoruid")]
        public Guid CreatorUid { get; set; }

        public TagModel() : base("tags")
        {
        }
    }
}
