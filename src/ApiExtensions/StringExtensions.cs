using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ApiExtensions
{
    public static class StringExtensions
    {
        public static string FirstCharacterToLower(this string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str, 0))
                return str;

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static bool NullableEquals(string str, string other)
        {
            return (str == null && other == null) || str != null && str.Equals(other);
        }

        public static string StripWhiteSpace(this string str)
        {
            return new string(str.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        public static T ToEnum<T>(this string str) where T : struct
        {
            if (!(typeof(T).GetTypeInfo().IsEnum))
                throw new InvalidOperationException("Needs to be an enum");

            return Enum.TryParse(str, true, out T naiveValue) ? naiveValue : default(T);
        }

        public static IEnumerable<string> Lines(this string source)
        {
            var process = source;
            var list = new List<string>();
            var nextEol = GetNextEol(process);
            do
            {
                list.Add(process.Substring(0, nextEol));
                process = process.Length > nextEol + 2 ? process.Substring(nextEol + 2) : string.Empty;
                nextEol = GetNextEol(process);
            } while (process.Length > 0);
            return list;
        }

        private static int GetNextEol(string source)
        {
            var nextEol = source.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            return nextEol > -1 ? nextEol : source.Length;
        }

        public static string NotEmpty(this string str)
        {
            return string.IsNullOrEmpty(str) ? throw new ArgumentNullException() : str;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
