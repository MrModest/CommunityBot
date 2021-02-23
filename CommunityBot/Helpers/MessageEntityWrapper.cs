using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CommunityBot.Helpers
{
    public class MessageEntityWrapper
    {
        private static readonly Dictionary<(MessageEntityType, ParseMode), (string prefix, string postfix)> TagDict = new()
        {
            [(MessageEntityType.Bold, ParseMode.Html)] = ("<b>", "</b>"),
            [(MessageEntityType.Italic, ParseMode.Html)] = ("<i>", "</i>"),
            [(MessageEntityType.Underline, ParseMode.Html)] = ("<u>", "</u>"),
            [(MessageEntityType.Strikethrough, ParseMode.Html)] = ("<s>", "</s>"),
            [(MessageEntityType.Code, ParseMode.Html)] = ("<code>", "</code>"),
            [(MessageEntityType.Pre, ParseMode.Html)] = ("<pre>", "</pre>"),
                
            [(MessageEntityType.TextLink, ParseMode.Html)] = ("<a href='{{url}}'>", "</a>"),
            [(MessageEntityType.TextLink, ParseMode.MarkdownV2)] = ("[", "]({{url}})"),
        };

        private static MessageEntityType[] SupportedEntityTypes => TagDict.Keys.Select(k => k.Item1).ToArray();

        private int StartIndex { get; }

        private int EndIndex { get; }

        private string Prefix { get; }

        private string Postfix { get; }

        private MessageEntityWrapper(MessageEntity entity, ParseMode parseMode)
        {
            StartIndex = entity.Offset;
            EndIndex = entity.Offset + entity.Length - 1;
            
            var (prefix, postfix) = TagDict.GetValueOrDefault((entity.Type, parseMode), ("", ""));
                
            if (entity.Type == MessageEntityType.TextLink && parseMode == ParseMode.Html)
            {
                Prefix = prefix.Replace("{{url}}", entity.Url);
            }
            else
            {
                Prefix = prefix;
            }
            
            if (entity.Type == MessageEntityType.TextLink && parseMode == ParseMode.MarkdownV2)
            {
                Postfix = postfix.Replace("{{url}}", entity.Url);
            }
            else
            {
                Postfix = postfix;
            }
        }

        private static (Dictionary<int,string[]> insertBefore, Dictionary<int,string[]> insertAfter) GetByEntities(MessageEntity[] entities, ParseMode parseMode)
        {
            var formattedEntities = entities
                .Where(e => e.Type.In(SupportedEntityTypes))
                .Select(e => new MessageEntityWrapper(e, parseMode))
                .ToArray();
            
            var insertBefore = formattedEntities
                .ToLookup(e => e.StartIndex, e => e.Prefix)
                .ToDictionary(e => e.Key, g => g.ToArray());
            
            // Reverse тут, чтобы "скобки" на одинаковой позиции закрывались в позиции обратной открываемой,
            // Дабы избежать ситуации <b><u>text</b></u>.
            var insertAfter = formattedEntities.Reverse()
                .ToLookup(e => e.EndIndex, e => e.Postfix)
                .ToDictionary(e => e.Key, g => g.ToArray());

            return (insertBefore, insertAfter);
        }
        
        public static string GetMarkupMessage(string plainText, MessageEntity[] entities, ParseMode parseMode)
        {
            var (insertBefore, insertAfter) = GetByEntities(entities, parseMode);

            var markupText = new StringBuilder();

            for (var i = 0; i < plainText.Length; i++)
            {
                var currentInsertBefore = insertBefore[i];
                var currentInsertAfter = insertAfter[i];
                
                if (currentInsertBefore.Any())
                {
                    markupText.Append(string.Join("", currentInsertBefore));
                }

                markupText.Append(plainText[i]);

                if (currentInsertAfter.Any())
                {
                    markupText.Append(string.Join("", currentInsertAfter));
                }
            }

            return markupText.ToString();
        }
    }
}