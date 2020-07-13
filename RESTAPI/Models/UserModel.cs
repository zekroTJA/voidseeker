using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public enum EmailConfirmStatus
    {
        UNSET,
        UNCONFIRMED,
        CONFIRMED,
    }

    /// <summary>
    /// User model.
    /// </summary>
    public class UserModel : UniqueModel
    {
        [RegularExpression(Constants.USERNAME_PATTERN, ErrorMessage = "invalid characters in username")]
        [StringLength(64, MinimumLength = 2, ErrorMessage = "invalid username length")]
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [StringLength(128)]
        [JsonPropertyName("displayname")]
        public string DisplayName { get; set; }

        [StringLength(4096)]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("lastlogin")]
        public DateTime? LastLogin { get; set; }

        [JsonPropertyName("isadmin")]
        public bool? IsAdmin { get; set; }

        [StringLength(256)]
        [RegularExpression(Constants.EMAIL_PATTERN)]
        [JsonPropertyName("emailaddress")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("emailconfirmstatus")]
        public EmailConfirmStatus EmailConfirmStatus { get; set; }


        [JsonIgnore]
        public string[] TagBlacklist { get; set; }

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
            EmailConfirmStatus = user.EmailConfirmStatus;
        }
    }
}
