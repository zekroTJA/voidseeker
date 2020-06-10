namespace RESTAPI.Extensions
{
    /// <summary>
    /// Collection of extension functions for the <see cref="string"/> class.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Returnes true when the string is null 
        /// or has a length of 0.
        /// </summary>
        /// <param name="s">string instance</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s) =>
            s == null || s.Length == 0;
    }
}
