using System;

namespace Panty
{
    public static class StringKit
    {
        private static readonly char[] specialSymbols = "!@#$%^&*()_+-=[]{}|;':\",./<>?".ToCharArray();
        private static ReadOnlySpan<char> SpecialCharSpan => specialSymbols.AsSpan();
        public static ReadOnlySpan<char> SliceToSpan(this string str, int start, int len) => str.AsSpan().Slice(start, len);
        public static bool ContainsSpecialSymbols(this ReadOnlySpan<char> source)
        {
            foreach (char c in source)
                foreach (char x in SpecialCharSpan)
                    if (c == x) return true;
            return false;
        }
    }
}