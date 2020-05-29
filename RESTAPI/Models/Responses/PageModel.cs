using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    public class PageModel<T>
    {
        [JsonPropertyName("offset")]
        public long Offset { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("data")]
        public List<T> Data { get; set; }

        public PageModel(List<T> data, int offset = 0)
        {
            Offset = offset;
            Size = data.Count;
            Data = data;
        }
    }
}
