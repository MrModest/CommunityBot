namespace CommunityBot.Helpers
{
    public static class MarkupExtensions
    {
        public static string EscapeMarkdown(this string raw)
        {
            return raw.Replace("_", "\\_");
        }

        public static string ToMdLink(this string rawLink, string linkText)
        {
            return $"[{linkText}]({rawLink})";
        }
    }
}