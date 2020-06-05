using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    public class UserDetailsModel : UserModel
    {
        [JsonPropertyName("imagescount")]
        public long ImagesCount { get; set; }

        public UserDetailsModel()
        {
        }

        public UserDetailsModel(UserModel user) : base(user)
        {
        }
    }
}
