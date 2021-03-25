using RESTAPI.Authorization;
using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public class RefreshTokenModel : EntityModel
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("useruid")]
        public Guid UserUid { get; set; }

        [JsonPropertyName("deadline")]
        public DateTime Deadline { get; set; }

        public RefreshTokenModel() : base("refreshtokens")
        {
        }

        public RefreshTokenModel(DeadlinedToken token, Guid userUid) : this()
        {
            Token = token.Token;
            Deadline = token.Deadline;
            UserUid = userUid;
        }

        public DeadlinedToken ToDeadlinedToken() =>
            new DeadlinedToken()
            {
                Token = Token,
                Deadline = Deadline,
            };
    }
}
