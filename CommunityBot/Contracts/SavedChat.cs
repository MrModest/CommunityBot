using Dapper.Contrib.Extensions;

namespace CommunityBot.Contracts
{
    [Table("SavedChats")]
    public class SavedChat : IEntity
    {
        public SavedChat(long chatId, string exactName, string joinLink)
        {
            ChatId = chatId;
            ExactName = exactName;
            JoinLink = joinLink;
        }

        //For Dapper
#pragma warning disable 8618
        public SavedChat()
#pragma warning restore 8618
        {
            
        }

        [Key]
        public long Id { get; set; }

        public long ChatId { get; }
        public string ExactName { get; }
        public string JoinLink { get; }
    }
}