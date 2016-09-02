using System;

namespace WebGallery.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether a specified substring occurs within this string by using a specified StringComparison
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substring"> a specified substring </param>
        /// <param name="comp"> an equality comparer to compare values </param>
        /// <returns></returns>
        public static bool Contains(this string str, string substring, StringComparison comp)
        {
            if (substring == null) return false;

            if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison", "comp");

            return str.IndexOf(substring, comp) >= 0;
        }
    }
}