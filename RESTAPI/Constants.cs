using System.Collections.Generic;

namespace RESTAPI
{
    /// <summary>
    /// Global application constants.
    /// </summary>
    public static class Constants
    {
        public const string REFRESH_TOKEN_COOKIE =       "void_refresh_token";
        public const string IMAGE_STORAGE_BUCKET =      "void-images";
        public const string THUMBNAIL_STORAGE_BUCKET =  "void-images-thumbnails";
        public const string MAIL_CONFIRM_SUBDIR =       "/confirmemail";
        public const string MAIL_PWRESET_SUBDIR =       "/passwordreset";
        public const string MAIL_CONFIRM_CACHE_KEY =    "mailconfirm";
        public const string MAIL_PWRESET_CACHE_KEY =    "mailpwreset";

        public const string TAG_PATTERN =               @"^[a-z0-9_\-']{1,64}$";
        public const string USERNAME_PATTERN =          @"^[a-z0-9-_\.]{1,32}$";
        public const string EMAIL_PATTERN =             @"^[\w-+]+@\w+.\w+$";

        public const string INVALID_ACCESS_TOKEN =      "invalid access token";

        public static readonly string[] ALLOWED_CONTENT_TYPES = {
            "image/bmp",
            "image/fif",
            "image/gif",
            "image/jpeg",
            "image/pict",
            "image/png",
            "image/tiff",
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
