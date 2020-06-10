using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    /// <summary>
    /// Tag model
    /// </summary>
    public class TagModel : UniqueModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creatoruid")]
        public Guid CreatorUid { get; set; }

        /// <summary>
        /// Create new empty <see cref="TagModel"/>.
        /// </summary>
        public TagModel() : base("tags")
        {
        }
    }
}
