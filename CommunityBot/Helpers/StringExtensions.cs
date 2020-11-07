namespace CommunityBot.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// String Null or Empty or WhiteSpace
        /// </summary>
        public static bool IsBlank(this string? value)
        {
            return string.IsNullOrWhiteSpace(value?.Trim());
        }

        /// <summary>
        /// String not Null or Empty or WhiteSpace
        /// </summary>
        public static bool IsNotBlank(this string? value)
        {
            return !string.IsNullOrWhiteSpace(value?.Trim());
        }
    }
}