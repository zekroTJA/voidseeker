using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    /// <summary>
    /// Response model to wrap a paged
    /// request response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageModel<T>
    {
        [JsonPropertyName("offset")]
        public long Offset { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("data")]
        public List<T> Data { get; set; }

        /// <summary>
        /// Create page model from list of data
        /// with the defined offset.
        /// </summary>
        /// <param name="data">data list</param>
        /// <param name="offset">offset</param>
        public PageModel(List<T> data, int offset = 0)
        {
            Offset = offset;
            Size = data.Count;
            Data = data;
        }
    }
}
