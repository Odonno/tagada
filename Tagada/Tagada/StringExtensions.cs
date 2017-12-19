namespace Tagada
{
    internal static class StringExtensions
    {
        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string LowerCapitalize(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return char.ToLower(s[0]) + s.Substring(1);
        }
    }
}
