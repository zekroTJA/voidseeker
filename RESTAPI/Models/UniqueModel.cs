using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public class UniqueModel
    {
        [JsonPropertyName("uid")]
        public Guid Uid { get; set; }

        [JsonPropertyName("index")]
        public string Index { get; private set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        public UniqueModel(string index)
        {
            Uid = Guid.NewGuid();
            Created = DateTime.Now;
            Index = index;
        }

        public UniqueModel AfterCreate()
        {
            Uid = Guid.NewGuid();
            Created = DateTime.Now;

            return this;
        }
    }
}
