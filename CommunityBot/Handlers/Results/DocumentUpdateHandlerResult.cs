using System;
using System.IO;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace CommunityBot.Handlers.Results
{
    public class DocumentUpdateHandlerResult : UpdateHandlerResultBase, IDisposable
    {
        private readonly Stream _content;

        public DocumentUpdateHandlerResult(long chatId, Stream content, string fileName, string caption, ParseMode parseMode = ParseMode.Default, int replyToMessageId = 0)
            : base(chatId, caption, parseMode, replyToMessageId)
        {
            _content = content;
            File = new InputOnlineFile(content, fileName);
        }

        public InputOnlineFile File { get; }

        public string Caption => Text;

        public void Dispose()
        {
            _content.Dispose();
        }
    }
}