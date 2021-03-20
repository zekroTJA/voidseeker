using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Authorization
{
    public struct DeadlinedToken
    {
        [JsonPropertyName("token")]
        public string Token;

        [JsonPropertyName("deadline")]
        public DateTime Deadline;

        public bool IsExpired(DateTime? now = null) =>
            (now ?? DateTime.Now) > Deadline;
    }
}
