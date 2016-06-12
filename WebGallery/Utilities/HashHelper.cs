using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebGallery.Utilities
{
    public class HashHelper
    {
        public static string CreateHash(Stream stream)
        {
            if (stream == null) return string.Empty;

            // it's very important for computing hash to set the position to the very begining within the stream.
            stream.Seek(0, SeekOrigin.Begin);

            var hash = SHA1.Create().ComputeHash(stream);

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }

    public static class StreamExtensions
    {
        public static bool MatchHash(this MemoryStream stream, string givenSha1Hash)
        {
            if (stream == null) return false;
            if (string.IsNullOrWhiteSpace(givenSha1Hash)) return false;

            return givenSha1Hash.Equals(HashHelper.CreateHash(stream), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}