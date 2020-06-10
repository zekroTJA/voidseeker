using System;
using System.Drawing;

namespace RESTAPI.Extensions
{
    /// <summary>
    /// Colelction of extension functions for the <see cref="Image"/> class.
    /// </summary>
    public static class ImageExtension
    {
        /// <summary>
        /// Calculate the scaled down aspect-ratio-reserved
        /// width and height of an image by max size.
        /// </summary>
        /// <param name="img">image instance</param>
        /// <param name="maxSize">maximum width or height</param>
        /// <returns></returns>
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
