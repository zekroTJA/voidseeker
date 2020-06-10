namespace RESTAPI.Hashing
{
    /// <summary>
    /// Provides simple functions for creating a hash from a
    /// password string and validating a password string by
    /// the corresponding hash.
    /// </summary>
    public interface IHasher
    {
        /// <summary>
        /// Creates a hash byte array form a
        /// passed string.
        /// </summary>
        /// <param name="password">password string</param>
        /// <returns></returns>
        byte[] Create(string password);

        /// <summary>
        /// Returns true when the hash generated from
        /// the passed password string matches the
        /// passed comparison hash.
        /// </summary>
        /// <param name="password">password string</param>
        /// <param name="hash">comparison hash</param>
        /// <returns></returns>
        bool Validate(string password, byte[] hash);
    }
}
