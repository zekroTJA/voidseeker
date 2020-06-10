using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    /// <summary>
    /// Response model extending <see cref="UserModel"/> by
    /// more detailed information.
    /// </summary>
    public class UserDetailsModel : UserModel
    {
        [JsonPropertyName("imagescount")]
        public long ImagesCount { get; set; }

        /// <summary>
        /// Initialize empty <see cref="UserDetailsModel"/>.
        /// </summary>
        public UserDetailsModel()
        {
        }

        /// <summary>
        /// Initialize new <see cref="UserDetailsModel"/> from
        /// passed <see cref="UserModel"/> instance.
        /// </summary>
        /// <param name="user">user model instance</param>
        public UserDetailsModel(UserModel user) : base(user)
        {
        }
    }
}
