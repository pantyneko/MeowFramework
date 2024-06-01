using System;

namespace Panty
{
    public static class StringKit
    {
        public static ReadOnlySpan<char> SliceToSpan(this string str, int start, int len) => str.AsSpan().Slice(start, len);
        public static bool ContainsSpecialSymbols(this string source)
        {
            foreach (char c in source)
                if (!char.IsLetterOrDigit(c)) return true;
            return false;
        }
        public static bool ContainsInvalidPathCharacters(this string path)
        {
            for (int i = 0, len = path.Length; i < len; i++)
                if (!ValidPathCharacter(path[i])) return true;
            return false;
        }
        private static bool ValidPathCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '\\' || c == '/' || c == '.';
        }
    }
}