using System;

namespace WebGallery.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether a specified substring occurs within this string by using a specified StringComparison.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substring">A specified substring</param>
        /// <param name="comp">An equality comparer to compare values</param>
        /// <returns></returns>
        public static bool Contains(this string str, string substring, StringComparison comp)
        {
            if (substring == null) return false;

            if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison", "comp");

            return str.IndexOf(substring, comp) >= 0;
        }

        /// <summary>
        /// Retrieves a substring start at the first character of the string with a spcified length and make it suffix with "..." if the string is longer than the specified length.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length">The specified length</param>
        /// <returns></returns>
        public static string ToShort(this string str, int length)
        {
            return str.Length > length ? str.Substring(0, length) + "..." : str;
        }
    }
}