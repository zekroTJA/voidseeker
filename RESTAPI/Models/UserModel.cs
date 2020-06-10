using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    /// <summary>
    /// User model.
    /// </summary>
    public class UserModel : UniqueModel
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("displayname")]
        public string DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("lastlogin")]
        public DateTime? LastLogin { get; set; }

        [JsonPropertyName("isadmin")]
        public bool? IsAdmin { get; set; }


        [JsonIgnore]
        public string EmailAddress { get; set; }

        [JsonIgnore]
        public byte[] PasswordHash { get; set; }

        /// <summary>
        /// Create new empty <see cref="UserModel"/>.
        /// </summary>
        public UserModel() : base("users")
        {
        }

        /// <summary>
        /// Create new <see cref="UserModel"/> as deep copy
        /// of the passed user model instance.
        /// </summary>
        /// <param name="user"></param>
        public UserModel(UserModel user) : base("users")
        {
            Uid = user.Uid;
            UserName = user.UserName;
            DisplayName = user.DisplayName;
            Description = user.Description;
            Created = user.Created;
            LastLogin = user.LastLogin;
            IsAdmin = user.IsAdmin;
            EmailAddress = user.EmailAddress;
            PasswordHash = user.PasswordHash;
        }
    }
}
