using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xunit;
using Moq;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using CommunityBot.Contracts;
using CommunityBot.Handlers;

namespace CommunityBotTests.Handlers
{
    public class RepostMessageUpdateHandlerTests
    {
        [Fact]
        public void RepostTextMessage()
        {
            const string repostMessage = "Simple text message for repost";
            const string chatJoinLink = "https://t.me/joinchat/12345";
            const string chatExactName = "TrueTestGroup";
            const long mainChannelId = -1001383589487;

            var update = GetReplyUpdate(repostMessage);
            
            var expectedText = $"{repostMessage}\n\n" + 
                               $" — <a href=\"tg://user?id={update.Message.From.Id}\">{update.Message.From.FirstName}</a> из <a href=\"{chatJoinLink}\">{chatExactName}</a>\n" +
                               $" — <a href=\"https://t.me/c/{update.Message.Chat.Id.ToString().Substring(4)}/{update.Message.MessageId}\">Источник</a>";

            var botClientMoq = new Mock<ITelegramBotClient>();
            var optionsMoq = new Mock<IOptions<BotConfigurationOptions>>();
            var loggerMoq = new Mock<ILogger<RepostMessageUpdateHandler>>();
            var chatRepMoq = new Mock<IChatRepository>();
            var mediaGroupServiceMoq = new Mock<IMediaGroupService>();
            
            chatRepMoq
                .Setup(r => r.GetByName(update.Message.Chat.Title))
                .Returns(Task.FromResult(new SavedChat(update.Message.Chat.Id, chatExactName, chatJoinLink)));

            optionsMoq
                .Setup(o => o.Value)
                .Returns(new BotConfigurationOptions {BotName = "PostFzCommunityBot", MainChannelId = mainChannelId});
            
            (ChatId chatId, string text, ParseMode parseMode, bool disableWebPagePreview) sendTextMessageAsyncArgs = (0, "", ParseMode.Default, false);
            
            botClientMoq
                .Setup(b => b.SendTextMessageAsync(It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()))
                .Callback<ChatId, string, ParseMode, bool, bool, int, IReplyMarkup, CancellationToken>(
                    (passedChatId, passedText, passedParseMode, passedWebPagePreviewFlag, _, _, _, _) =>
                    {
                        sendTextMessageAsyncArgs.chatId = passedChatId;
                        sendTextMessageAsyncArgs.text = passedText;
                        sendTextMessageAsyncArgs.parseMode = passedParseMode;
                        sendTextMessageAsyncArgs.disableWebPagePreview = passedWebPagePreviewFlag;
                    });

            var handler = new RepostMessageUpdateHandler(
                botClientMoq.Object,
                optionsMoq.Object,
                loggerMoq.Object,
                chatRepMoq.Object,
                mediaGroupServiceMoq.Object);

            handler.HandleUpdateAsync(update).Wait();

            Assert.Equal(mainChannelId.ToString(), sendTextMessageAsyncArgs.chatId.ToString());
            Assert.Equal(expectedText, sendTextMessageAsyncArgs.text);
            Assert.Equal(ParseMode.Html, sendTextMessageAsyncArgs.parseMode);
            Assert.True(sendTextMessageAsyncArgs.disableWebPagePreview);
        }

        private static Update GetReplyUpdate(string repostMessage)
        {
            var chat = new Chat
            {
                Id = -1001151694363,
                Title = "TrueTestGroup",
                Type = ChatType.Supergroup
            };
            
            var postAuthor = new User
            {
                Id = 1234590,
                IsBot = false,
                FirstName = "MyName",
                Username = "myUserName"
            };

            var repliedUser = postAuthor;
            
            return new Update
            {
                Message = new Message
                {
                    From = repliedUser,
                    Chat = chat,
                    ReplyToMessage = new Message
                    {
                        From = postAuthor,
                        Chat = chat,
                        Text = repostMessage
                    },
                    Text = "@PostFzCommunityBot",
                    Entities = new []
                    {
                        new MessageEntity
                        {
                            Type = MessageEntityType.Mention,
                            Offset = 0,
                            Length = 19
                        }
                    }
                }
            };
        }
    }
}