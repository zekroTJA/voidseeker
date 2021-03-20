using RESTAPI.Authorization;
using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    public class LoginResponseModel : UserModel
    {
        [JsonPropertyName("accesstoken")]
        public DeadlinedToken AccessToken { get; set; }

        public LoginResponseModel(UserModel user, DeadlinedToken token) : base(user)
        {
            AccessToken = token;
        }
    }
}
