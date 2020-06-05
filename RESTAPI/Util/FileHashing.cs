using System;
using System.IO;
using System.Security.Cryptography;

namespace RESTAPI.Util
{
    public class FileHashing
    {
        public static string GetHash(Stream fileStream)
        {
            var byteHash = MD5.Create().ComputeHash(fileStream);
            return BitConverter.ToString(byteHash).Replace("-", "").ToLower();
        }
    }
}
