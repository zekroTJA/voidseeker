using Isopoh.Cryptography.Argon2;
using System.Text;

namespace RESTAPI.Hashing
{
    /// <summary>
    /// Implementation of <see cref="IHasher"/> using Argon2ID.
    /// </summary>
    public class Argon2Hasher : IHasher
    {
        public byte[] Create(string password) =>
            Encoding.UTF8.GetBytes(Argon2.Hash(password));

        public bool Validate(string password, byte[] hash) =>
            Argon2.Verify(Encoding.UTF8.GetString(hash), password);
    }
}
