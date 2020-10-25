using System;

namespace CommunityBot.Helpers
{
    public static class MarkupExtensions
    {
        public static string EscapeMarkdown(this string raw)
        {
            return raw.Replace("_", "\\_");
        }

        [Obsolete("Телеграм может ругаться на некоторые символы разметки в тексте")]
        public static string ToMdLink(this string rawLink, string linkText)
        {
            return $"[{linkText}]({rawLink})";
        }

        public static string ToHtmlLink(this string rawLink, string linkText)
        {
            return $"<a href=\"{rawLink}\">{linkText}</a>";
        }
    }
}