using System;
using System.Security.Cryptography;

namespace RESTAPI.Util
{
    public class CryptoRandomUtil
    {
        public static string GetBase64String(int len)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[len];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }
    }
}
