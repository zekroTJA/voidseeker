using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Authorization
{
    public class DeadlinedToken
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("deadline")]
        public DateTime Deadline { get; set; }

        public bool IsExpired(DateTime? now = null) =>
            (now ?? DateTime.Now) > Deadline;
    }
}
