using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CommunityBot.Helpers
{
    public static class BotClientExtensions
    {
        public static async Task<string> DownloadStringFile(this ITelegramBotClient botClient, string fileId)
        {
            await using var stream = new MemoryStream();
            await botClient.GetInfoAndDownloadFileAsync(fileId, stream);

            if (stream.Position != 0) 
            {
                if (!stream.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"Can't seek file '{fileId}'!");
                }
                stream.Position = 0;
            }

            return await new StreamReader(stream).ReadToEndAsync();
        }
    }
}