using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public class UniqueModel
    {
        [JsonPropertyName("uid")]
        public Guid UID { get; set; }

        [JsonPropertyName("index")]
        public string Index { get; set; }

        public UniqueModel(string index)
        {
            UID = Guid.NewGuid();
            Index = index;
        }
    }
}
