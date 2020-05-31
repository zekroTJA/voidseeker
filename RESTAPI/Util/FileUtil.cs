namespace RESTAPI.Util
{
    public static class FileUtil
    {
        public static string? GetFileExtension(string fileName)
        {
            var i = fileName.LastIndexOf('.');
            if (i < 0)
                return null;

            return fileName.Substring(i + 1);
        }
    }
}
