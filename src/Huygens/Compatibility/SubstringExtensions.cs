using System;

namespace Huygens.Compatibility
{
    internal static class SubstringExtensions
    {
        /// <summary>
        /// Return the substring up to but not including the first instance of character 'c'.
        /// If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBefore(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.IndexOf(c));
            return idx < 0 ? src : src.Substring(0, idx);
        }


        /// <summary>
        /// Return the substring up to but not including the first instance of string 's'.
        /// If 's' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBefore(this string src, string s, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.IndexOf(s, stringComparison));
            return idx < 0 ? src : src.Substring(0, idx);
        }


        /// <summary>
        /// Return the substring up to but not including the last instance of character 'c'.
        /// If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBeforeLast(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.LastIndexOf(c));
            return idx < 0 ? src : src.Substring(0, idx);
        }


        /// <summary>
        /// Return the substring up to but not including the last instance of string 's'.
        /// If 's' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBeforeLast(this string src, string s, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.LastIndexOf(s, stringComparison));
            return idx < 0 ? src : src.Substring(0, idx);
        }

        /// <summary>
        /// Return the substring after to but not including the first instance of character 'c'.
        /// If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfter(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.IndexOf(c) + 1);
            return idx < 0 ? src : src.Substring(idx);
        }

        /// <summary>
        /// Return the substring after to but not including the first instance of string 's'.
        /// If 's' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfter(this string src, string s, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.ExtendedIndexOf(s, s.Length, stringComparison));
            return idx < 0 ? src : src.Substring(idx);
        }

        /// <summary>
        /// Return index of target + offset, or -1 if not found.
        /// </summary>
        public static int ExtendedIndexOf(this string src, string target, int offset, StringComparison stringComparison) {
            var idx = src.IndexOf(target, stringComparison);
            return idx < 0 ? idx : idx + offset;
        }

        /// <summary>
        /// Return the substring after to but not including the last instance of character 'c'.
        /// If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfterLast(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.LastIndexOf(c) + 1);
            return idx < 0 ? src : src.Substring(idx);
        }

        /// <summary>
        /// Return the substring after to but not including the last instance of string 's'.
        /// If 's' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfterLast(this string src, string s, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(src)) return "";

            var idx = Math.Min(src.Length, src.ExtendedIndexOf(s, s.Length, stringComparison));
            return idx < 0 ? src : src.Substring(idx);
        }
    }
}