using System;
using System.Security;

namespace RESTAPI.Util
{
    public class SecureStringUtil
    {
        /// <summary>
        /// Create a new <see cref="SecureString"/> from 
        /// passed string.
        /// </summary>
        /// <param name="str">content</param>
        /// <param name="readOnly">set to read only</param>
        /// <returns></returns>
        public static SecureString FromString(string str, bool readOnly = true)
        {
            var secStr = new SecureString();
            Array.ForEach(str.ToCharArray(), secStr.AppendChar);

            if (readOnly) secStr.MakeReadOnly();

            return secStr;
        }
    }
}
