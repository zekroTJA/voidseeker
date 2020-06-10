using System;
using System.IO;
using System.Security.Cryptography;

namespace RESTAPI.Util
{
    /// <summary>
    /// Provides general file hashing utilities.
    /// </summary>
    public static class FileHashing
    {
        /// <summary>
        /// Creates an MD5 hash string by passed file stream.
        /// </summary>
        /// <param name="fileStream">file stream</param>
        /// <returns></returns>
        public static string GetHash(Stream fileStream)
        {
            var byteHash = MD5.Create().ComputeHash(fileStream);
            return BitConverter.ToString(byteHash).Replace("-", "").ToLower();
        }
    }
}
