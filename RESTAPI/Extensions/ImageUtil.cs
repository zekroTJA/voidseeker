using System;
using System.Drawing;

namespace RESTAPI.Extensions
{
    public static class ImageUtil
    {
        public static (int width, int height) ShrinkSize(this Image img, int maxSize)
        {
            int width, height;

            if (img.Width > img.Height)
            {
                width = maxSize;
                height = (int)Math.Floor((double)maxSize / img.Width * img.Height);
            }
            else
            {
                height = maxSize;
                width = (int)Math.Floor((double)maxSize / img.Height * img.Width);
            }

            return (width, height);
        }
    }
}
