namespace RESTAPI.Extensions
{
    public static class StringExtension
    {
        public static bool NullOrEmpty(this string s) =>
            s == null || s.Length == 0;
    }
}
