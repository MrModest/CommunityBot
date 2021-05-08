using System.Collections.Generic;
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
using CommunityBot.Handlers.Results;

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

        public static IEnumerable<object[]> RepostTextMessageTestData
        {
            get
            {
                yield return new object[]
                {
                    "Post text with bold and italic text. Also it have an inline link.",
                    "Post text with <b>bold</b> and <i>italic</i> text. Also it have an <a href='https://github.com/'>inline link</a>.",
                    new []
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
                    }
                };

                yield return new object[]
                {
                    "Simple text message for repost"
                };
                
                yield return new object[]
                {
                    "Text which have spec symbols like a <...>",
                    "Text which have spec symbols like a &#60;...&#62;"
                };
            }
        }

        [Theory]
        [MemberData(nameof(RepostTextMessageTestData))]
        public void RepostFormattingTextMessage(string repostMessage, string? repostFormatMessage = null, MessageEntity[]? entities = null)
        {
            var update = GetTextReplyUpdate(repostMessage, entities);
            
            var expectedText = $"{repostFormatMessage ?? repostMessage}\n\n" +
                               $"{GetFooterText(update)}";

            var handler = GetRepostHandler(update);

            var resultTask = handler.HandleUpdateAsync(update);

            resultTask.Wait();

            var result = (TextUpdateHandlerResult)resultTask.Result;

            Assert.Equal(MainChannelId.ToString(), result.ChatId);
            Assert.Equal(expectedText, result.MessageText);
            Assert.Equal(ParseMode.Html, result.ParseMode);
            Assert.True(result.DisableWebPagePreview);
        }
        
        [Fact]
        public void RepostPhotoMessage()
        {
            const string repostMessage = "Simple photo message for repost";

            var update = GetPhotoReplyUpdate(repostMessage);

            var expectedPhotoFileId = TestPhoto.OrderByDescending(ps => ps.FileSize).First().FileId;
            var expectedCaption = $"{repostMessage}\n\n" +
                                  $"{GetFooterText(update)}";

            var handler = GetRepostHandler(update);

            var resultTask = handler.HandleUpdateAsync(update);

            resultTask.Wait();

            var result = (PhotoUpdateHandlerResult)resultTask.Result;
            
            Assert.Equal(MainChannelId.ToString(), result.ChatId);
            Assert.Equal(expectedPhotoFileId, result.FileId);
            Assert.Equal(expectedCaption, result.Caption);
            Assert.Equal(ParseMode.Html, result.ParseMode);
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
            IOptions<BotConfigurationOptions>? options = null,
            ILogger<RepostMessageUpdateHandler>? logger = null,
            IChatRepository? chatRepository = null,
            IMediaGroupService? mediaGroupService = null)
        {
            options ??= GetBotConf();
            logger ??= new Mock<ILogger<RepostMessageUpdateHandler>>().Object;
            chatRepository ??= GetChatRep(update);
            mediaGroupService ??= new Mock<IMediaGroupService>().Object;
            
            return new RepostMessageUpdateHandler(
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
