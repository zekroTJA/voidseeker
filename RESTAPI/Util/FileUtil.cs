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

        public static string GetFileExtensionByContentType(string contentType)
        {
            contentType = contentType.ToLower();

            if (!Constants.FILE_EXTENSIONS_BY_CONTENT_TYPE.ContainsKey(contentType))
                return "unknown";

            return Constants.FILE_EXTENSIONS_BY_CONTENT_TYPE[contentType];
        }
    }
}
