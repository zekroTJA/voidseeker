using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    public class CompletionWrapperModel<T>
    {
        [JsonPropertyName("uid")]
        public Guid Uid { get; set; }

        [JsonPropertyName("initialized")]
        public DateTime Initialized { get; set; }

        [JsonPropertyName("deadline")]
        public DateTime Deadline { get; set; }

        [JsonPropertyName("completionresource")]
        public string CompletionResource { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
