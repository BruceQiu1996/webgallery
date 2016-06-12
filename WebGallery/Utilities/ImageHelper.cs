using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WebGallery.Utilities
{
    public class ImageHelper
    {
        public static Image FromStream(Stream stream)
        {
            if (stream == null) return null;

            stream.Seek(0, SeekOrigin.Begin);

            return Image.FromStream(stream);
        }
    }

    public static class ImageExtensions
    {
        public static bool IsPng(this Image img)
        {
            if (img == null) return false;

            return img.RawFormat.Guid == ImageFormat.Png.Guid;
        }
    }
}