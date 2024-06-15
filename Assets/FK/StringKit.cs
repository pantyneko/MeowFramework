using System;
using System.Text.RegularExpressions;

namespace Panty
{
    public static class StringKit
    {
        public static ReadOnlySpan<char> SliceToSpan(this string str, int start, int len) => str.AsSpan().Slice(start, len);
        public static bool ContainsSpecialSymbols(this string source)
        {
            for (int i = 0, len = source.Length; i < len; i++)
                if (!char.IsLetterOrDigit(source[i])) return true;
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
        public static string RemoveSpecialCharacters(this string input)
        {
            // 使用正则表达式替换所有非字母、数字和中文字符
            return Regex.Replace(input, "[^a-zA-Z0-9\u4e00-\u9fa5]", "");
        }
    }
}