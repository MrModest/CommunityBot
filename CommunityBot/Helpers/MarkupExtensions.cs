using System;

namespace CommunityBot.Helpers
{
    public static class MarkupExtensions
    {
        public static string EscapeHtml(this string raw)
        {
            return raw.Replace("\n", "<br />");
        }

        public static string ToHtmlLink(this string rawLink, string linkText)
        {
            return $"<a href=\"{rawLink}\">{linkText}</a>";
        }

        public static string ToMonospace(this string raw)
        {
            return $"<code>{raw}</code>";
        }
    }
}