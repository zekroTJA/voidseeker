using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public class UserModel : UniqueModel
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("displayname")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("lastlogin")]
        public DateTime? LastLogin { get; set; }

        [JsonPropertyName("isadmin")]
        public bool IsAdmin { get; set; }


        [JsonIgnore]
        public string? EmailAddress { get; set; }

        [JsonIgnore]
        public byte[] PasswordHash { get; set; }

        public UserModel() : base("users")
        {
        }

        public UserModel(UserModel user) : base("users")
        {
            UID = user.UID;
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
