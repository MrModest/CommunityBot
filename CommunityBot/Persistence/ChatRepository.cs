using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Newtonsoft.Json;

namespace CommunityBot.Persistence
{
    public class ChatRepository : IChatRepository
    {
        public async Task<SavedChat?> GetByName(string chatExactName)
        {
            var savedChats = await ReadFromFile();
            return savedChats.FirstOrDefault(c => c.ExactName == chatExactName);
        }

        public async Task<SavedChat?> GetById(long chatId)
        {
            var savedChats = await ReadFromFile();
            return savedChats.FirstOrDefault(c => c.ChatId == chatId);
        }

        public async Task<SavedChat> AddOrUpdate(SavedChat savedChat)
        {
            var savedChats = await ReadFromFile();
            var newSavedChats = savedChats.Concat(new[] {savedChat});
            var json = JsonConvert.SerializeObject(newSavedChats);
            await WriteToFile(json);
            return savedChat;
        }

        public async Task<SavedChat?> Remove(string chatExactName)
        {
            var savedChats = await ReadFromFile();
            var deletedChat = savedChats.FirstOrDefault(c => c.ExactName == chatExactName);
            var newSavedChats = savedChats.Except(new []{ deletedChat });
            var json = JsonConvert.SerializeObject(newSavedChats);
            await WriteToFile(json);
            return deletedChat;
        }

        private static async Task WriteToFile(string json)
        {
            await using var file = new StreamWriter(File.Open(AppContext.BaseDirectory, FileMode.OpenOrCreate, FileAccess.Write));
            await file.WriteAsync(json);
        }

        private static async Task<SavedChat[]> ReadFromFile()
        {
            using var file = new StreamReader(File.Open(AppContext.BaseDirectory, FileMode.OpenOrCreate, FileAccess.Read));
            var json = await file.ReadToEndAsync();

            return JsonConvert.DeserializeObject<SavedChat[]>(json);
        }
    }
}