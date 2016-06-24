using Ionic.Zip;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WebGallery.Utilities
{
    public class ZipFileHelper
    {
        public static ZipFile FromStream(Stream stream)
        {
            if (stream == null) return null;

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                return ZipFile.Read(stream);
            }
            catch
            {
                return null;
            }
        }
    }

    public static class ZipFileExtensions
    {
        public static bool ContainsEntry(this ZipFile zip, string name, bool ignoreCase)
        {
            if (zip == null) return false;
            if (name == null) return false;

            return zip.EntryFileNames.Contains(name, StringComparer.Create(CultureInfo.CurrentCulture, ignoreCase));
        }
    }
}