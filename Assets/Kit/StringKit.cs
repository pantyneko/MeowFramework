namespace Panty
{
    public static class StringKit
    {
        private readonly static char[] SpecialChars = @"[!！?？<>,，、。.、@#$%^&*()\/]".ToCharArray();
        public static int GetSpecialCharsCount(this string str) => str.IndexOfAny(SpecialChars);
        public static bool HaveSpecialChar(this string str) => str.IndexOfAny(SpecialChars) >= 0;
    }
}