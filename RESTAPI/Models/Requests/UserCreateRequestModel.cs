﻿using RESTAPI.Extensions;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RESTAPI.Models.Requests
{
    /// <summary>
    /// Request body model used for creating and
    /// updating users.
    /// </summary>
    public class UserCreateRequestModel : UserModel
    {
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("oldpassword")]
        public string? OldPassword { get; set; }

        [JsonPropertyName("emailaddress")]
        public new string EmailAddress { get; set; }

        /// <summary>
        /// Returns true if the username
        /// set is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValidUsername() =>
            Regex.IsMatch(UserName, Constants.USERNAME_PATTERN);

        /// <summary>
        /// Returns true if the set password
        /// is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValidPassword() =>
            !Password.IsNullOrEmpty();
    }
}
