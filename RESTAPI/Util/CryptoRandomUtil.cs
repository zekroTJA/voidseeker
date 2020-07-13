using System;
using System.Security.Cryptography;

namespace RESTAPI.Util
{
    public class CryptoRandomUtil
    {
        public static string GetBase64String(int len, bool optimizeForUrls = true)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[len];
                rng.GetBytes(tokenData);
                var res = Convert.ToBase64String(tokenData);

                if (optimizeForUrls)
                    res = res.Replace('=', '_')
                             .Replace('+', '-');

                return res;
            }
        }
    }
}
