using System;

namespace RESTAPI.Authorization
{
    /// <summary>
    /// Provides functionalities to create a session
    /// key with given AuthClaims and validating a
    /// passed session key.
    /// </summary>
    public interface IAuthorization
    {
        /// <summary>
        /// Returns the defined time span until a session
        /// key must have been expired.
        /// </summary>
        /// <returns></returns>
        TimeSpan GetExpiration();

        /// <summary>
        /// Creates a session key which can be used to
        /// securely authenticate a users HTTP session and
        /// get the passed AuthClaims back.
        /// </summary>
        /// <param name="properties">AuthClaims to generate session key with</param>
        /// <returns>session key</returns>
        string GetSessionKey(AuthClaims properties);

        /// <summary>
        /// Validates the passed session key string.
        /// If this succeeds, the AuthClaims used to
        /// generate this key must be recovered and
        /// returned. If this fails, the method must
        /// throw an exception of any type.
        /// </summary>
        /// <param name="key">session key</param>
        /// <returns>recovered AuthClaims</returns>
        AuthClaims ValidateSessionKey(string key);
    }

}
