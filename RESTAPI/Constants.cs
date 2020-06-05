namespace RESTAPI
{
    public static class Constants
    {
        public const string SESSION_COOKIE_NAME = "void_session_key";
        public const string IMAGE_STORAGE_BUCKET = "void-images";
        public const string THUMBNAIL_STORAGE_BUCKET = "void-images-thumbnails";

        public static readonly string[] ALLOWED_CONTENT_TYPES = {
            "image/bmp",
            "image/fif",
            "image/gif",
            "image/jpeg",
            "image/pict",
            "image/png",
            "image/tiff",
            // "image/florian", ???
        };
    }
}
