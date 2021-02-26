using System.Linq;
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
using Telegram.Bot.Types.InputFiles;

using CommunityBot.Contracts;
using CommunityBot.Handlers;

namespace CommunityBotTests.Handlers
{
    public class RepostMessageUpdateHandlerTests
    {
        private const string ChatJoinLink = "https://t.me/joinchat/12345";
        private const string ChatExactName = "TrueTestGroup";
        private const long MainChannelId = -1001383589487;

        private static readonly PhotoSize[] TestPhoto = {
            new()
            {
                FileId =
                    "AgACAgIAAxkBAAEIYehgOLjZydUv3ZnVb_XDFmK_cdnwiAACv7IxG5WSyUlSqU7INNO2EuZNNJ8uAAMBAAMCAANtAANgdwACHgQ",
                FileUniqueId = "AQAD5k00ny4AA2B3AAI",
                FileSize = 6256,
                Width = 320,
                Height = 320
            },
            new()
            {
                FileId =
                    "AgACAgIAAxkBAAEIYehgOLjZydUv3ZnVb_XDFmK_cdnwiAACv7IxG5WSyUlSqU7INNO2EuZNNJ8uAAMBAAMCAAN4AANedwACHgQ",
                FileUniqueId = "AQAD5k00ny4AA153AAI",
                FileSize = 19694,
                Width = 800,
                Height = 800
            },
            new()
            {
                FileId =
                    "AgACAgIAAxkBAAEIYehgOLjZydUv3ZnVb_XDFmK_cdnwiAACv7IxG5WSyUlSqU7INNO2EuZNNJ8uAAMBAAMCAAN5AANddwACHgQ",
                FileUniqueId = "AQAD5k00ny4AA113AAI",
                FileSize = 24987,
                Width = 1200,
                Height = 1200
            }
        };
        
        [Fact]
        public void RepostTextMessage()
        {
            const string repostMessage = "Simple text message for repost";
            
            var update = GetTextReplyUpdate(repostMessage);

            var expectedText = $"{repostMessage}\n\n" +
                               $"{GetFooterText(update)}";
                               
            var botClientMoq = new Mock<ITelegramBotClient>();

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

            var handler = GetRepostHandler(update, botClient: botClientMoq.Object);

            handler.HandleUpdateAsync(update).Wait();

            Assert.Equal(MainChannelId.ToString(), sendTextMessageAsyncArgs.chatId.ToString());
            Assert.Equal(expectedText, sendTextMessageAsyncArgs.text);
            Assert.Equal(ParseMode.Html, sendTextMessageAsyncArgs.parseMode);
            Assert.True(sendTextMessageAsyncArgs.disableWebPagePreview);
        }

        [Fact]
        public void RepostPhotoMessage()
        {
            const string repostMessage = "Simple photo message for repost";

            var update = GetPhotoReplyUpdate(repostMessage);

            var expectedPhotoFileId = TestPhoto.OrderByDescending(ps => ps.FileSize).First().FileId;
            var expectedCaption = $"{repostMessage}\n\n" +
                                  $"{GetFooterText(update)}";
            
            var botClientMoq = new Mock<ITelegramBotClient>();

            (ChatId chatId, InputOnlineFile photo, string caption, ParseMode parseMode) sendTextMessageAsyncArgs = (0, "", "", ParseMode.Default);
            
            botClientMoq
                .Setup(b => b.SendPhotoAsync(It.IsAny<ChatId>(), It.IsAny<InputOnlineFile>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
                    It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()))
                .Callback<ChatId, InputOnlineFile, string, ParseMode, bool, int, IReplyMarkup, CancellationToken>(
                    (passedChatId, passedPhoto, passedCaption, passedParseMode, _, _, _, _) =>
                    {
                        sendTextMessageAsyncArgs.chatId = passedChatId;
                        sendTextMessageAsyncArgs.photo = passedPhoto;
                        sendTextMessageAsyncArgs.caption = passedCaption;
                        sendTextMessageAsyncArgs.parseMode = passedParseMode;
                    });

            var handler = GetRepostHandler(update, botClient: botClientMoq.Object);

            handler.HandleUpdateAsync(update).Wait();
            
            Assert.Equal(MainChannelId.ToString(), sendTextMessageAsyncArgs.chatId.ToString());
            Assert.Equal(expectedPhotoFileId, sendTextMessageAsyncArgs.photo.FileId);
            Assert.Equal(expectedCaption, sendTextMessageAsyncArgs.caption);
            Assert.Equal(ParseMode.Html, sendTextMessageAsyncArgs.parseMode);
        }

        [Fact]
        public void RepostFormattingTextMessage()
        {
            const string repostMessage = "Post text with bold and italic text. Also it have an inline link.";
            const string repostFormatMessage = "Post text with <b>bold</b> and <i>italic</i> text. Also it have an <a href='https://github.com/'>inline link</a>.";
            
            var entities = new []
            {
                new MessageEntity
                {
                    Offset = 15,
                    Length = 4,
                    Type = MessageEntityType.Bold
                },
                new MessageEntity
                {
                    Offset = 24,
                    Length = 6,
                    Type = MessageEntityType.Italic
                },
                new MessageEntity
                {
                    Offset = 53,
                    Length = 11,
                    Type = MessageEntityType.TextLink,
                    Url = "https://github.com/"
                }
            };
            
            var update = GetTextReplyUpdate(repostMessage, entities);
            
            var expectedText = $"{repostFormatMessage}\n\n" +
                               $"{GetFooterText(update)}";
                               
            var botClientMoq = new Mock<ITelegramBotClient>();

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

            var handler = GetRepostHandler(update, botClient: botClientMoq.Object);

            handler.HandleUpdateAsync(update).Wait();

            Assert.Equal(MainChannelId.ToString(), sendTextMessageAsyncArgs.chatId.ToString());
            Assert.Equal(expectedText, sendTextMessageAsyncArgs.text);
            Assert.Equal(ParseMode.Html, sendTextMessageAsyncArgs.parseMode);
            Assert.True(sendTextMessageAsyncArgs.disableWebPagePreview);
        }
        
        private static Update GetTextReplyUpdate(string repostMessage, MessageEntity[]? entities = null)
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
                        Text = repostMessage,
                        Entities = entities
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

        private static Update GetPhotoReplyUpdate(string repostMessage)
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
                        Photo = TestPhoto,
                        Caption = repostMessage
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

        private static string GetFooterText(Update update)
        {
            return $" — <a href=\"tg://user?id={update.Message.From.Id}\">{update.Message.From.FirstName}</a> из <a href=\"{ChatJoinLink}\">{ChatExactName}</a>\n" +
                   $" — <a href=\"https://t.me/c/{update.Message.Chat.Id.ToString().Substring(4)}/{update.Message.MessageId}\">Источник</a>";
        }

        private static RepostMessageUpdateHandler GetRepostHandler(Update update,
            ITelegramBotClient? botClient = null,
            IOptions<BotConfigurationOptions>? options = null,
            ILogger<RepostMessageUpdateHandler>? logger = null,
            IChatRepository? chatRepository = null,
            IMediaGroupService? mediaGroupService = null)
        {
            botClient ??= new Mock<ITelegramBotClient>().Object;
            options ??= GetBotConf();
            logger ??= new Mock<ILogger<RepostMessageUpdateHandler>>().Object;
            chatRepository ??= GetChatRep(update);
            mediaGroupService ??= new Mock<IMediaGroupService>().Object;
            
            return new RepostMessageUpdateHandler(
                botClient,
                options,
                logger,
                chatRepository,
                mediaGroupService);
        }

        private static IChatRepository GetChatRep(Update update)
        {
            var chatRepMoq = new Mock<IChatRepository>();
            
            chatRepMoq
                .Setup(r => r.GetByName(update.Message.Chat.Title))
                .Returns(Task.FromResult<SavedChat?>(new SavedChat(update.Message.Chat.Id, ChatExactName, ChatJoinLink)));

            return chatRepMoq.Object;
        }

        private static IOptions<BotConfigurationOptions> GetBotConf()
        {
            var optionsMoq = new Mock<IOptions<BotConfigurationOptions>>();
            
            optionsMoq
                .Setup(o => o.Value)
                .Returns(new BotConfigurationOptions {BotName = "PostFzCommunityBot", MainChannelId = MainChannelId});

            return optionsMoq.Object;
        }
    }
}