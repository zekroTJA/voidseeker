using System.Collections.Generic;

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

        public static readonly Dictionary<string, string> FILE_EXTENSIONS_BY_CONTENT_TYPE = new Dictionary<string, string>()
        {
            { "image/bmp",  "bmp"  },
            { "image/fif",  "jfif" },
            { "image/gif",  "gif"  },
            { "image/jpeg", "jpeg" },
            { "image/pict", "pict" },
            { "image/png",  "png"  },
            { "image/tiff", "tiff" },
        };
    }
}
