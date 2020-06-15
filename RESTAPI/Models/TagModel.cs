using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    /// <summary>
    /// Tag model
    /// </summary>
    public class TagModel : UniqueModel
    {
        [JsonPropertyName("name")]
        [RegularExpression(Constants.TAG_PATTERN)]
        public string Name { get; set; }

        [JsonPropertyName("creatoruid")]
        public Guid CreatorUid { get; set; }

        [JsonPropertyName("coupledwith")]
        public string[] CoupledWith { get; set; }

        /// <summary>
        /// Create new empty <see cref="TagModel"/>.
        /// </summary>
        public TagModel() : base("tags")
        {
        }
    }
}
