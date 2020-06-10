namespace RESTAPI.Util
{
    /// <summary>
    /// Generic file utility functions.
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Returns the file extension of a passed
        /// file path and name or null, if the file
        /// name has no extension.
        /// </summary>
        /// <param name="fileName">file path and name</param>
        /// <returns></returns>
        public static string? GetFileExtension(string fileName)
        {
            var i = fileName.LastIndexOf('.');
            if (i < 0)
                return null;

            return fileName.Substring(i + 1);
        }

        /// <summary>
        /// Retrurns a file extension by passed MIME type.
        /// If no extension matches the passed MIME type,
        /// "unknown" will be returned.
        /// </summary>
        /// <param name="contentType">file MIME type</param>
        /// <returns></returns>
        public static string GetFileExtensionByContentType(string contentType)
        {
            contentType = contentType.ToLower();

            if (!Constants.FILE_EXTENSIONS_BY_CONTENT_TYPE.ContainsKey(contentType))
                return "unknown";

            return Constants.FILE_EXTENSIONS_BY_CONTENT_TYPE[contentType];
        }
    }
}
