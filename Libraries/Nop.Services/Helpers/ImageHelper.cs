using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Nop.Services.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Removes EXIF rotation from an image
        /// </summary>
        /// <param name="binData"></param>
        /// <returns></returns>
        public static byte[] RemoveEXIFRotation(byte[] binData, string contentType = "")
        {
            using (var stream = new MemoryStream(binData))
            {
                Bitmap bmp = new Bitmap(stream);

                if (Array.IndexOf(bmp.PropertyIdList, 274) > -1)
                {
                    var orientation = (int)bmp.GetPropertyItem(274).Value[0];
                    switch (orientation)
                    {
                        case 1:
                            // No rotation required.
                            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                            break;
                        case 2:
                            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case 3:
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 4:
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                            break;
                        case 5:
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case 6:
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 7:
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case 8:
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                    // This EXIF data is now invalid and should be removed.
                    bmp.RemovePropertyItem(274);
                }

                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormatByContentType(contentType));
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Get ImageFormat from content type
        /// </summary>
        /// <param name="contentType">Content Type of Images</param>
        /// <returns></returns>
        public static ImageFormat ImageFormatByContentType(string contentType)
        {
            var result = ImageFormat.Jpeg;
            if (contentType.Contains("png"))
                result = ImageFormat.Png;
            else if (contentType.Contains("gif"))
                result = ImageFormat.Gif;
            else if (contentType.Contains("bmp"))
                result = ImageFormat.Bmp;
            else if (contentType.Contains("tiff"))
                result = ImageFormat.Tiff;
            return result;
        }
    }
}
