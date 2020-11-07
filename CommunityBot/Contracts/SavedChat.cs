namespace CommunityBot.Contracts
{
    [Dapper.Contrib.Extensions.Table("SavedChats")]
    public class SavedChat : EntityBase
    {
        public SavedChat(long chatId, string exactName, string joinLink)
        {
            ChatId = chatId;
            ExactName = exactName;
            JoinLink = joinLink;
        }

        //For Dapper
        public SavedChat()
        {
            
        }

        public long ChatId { get; }
        public string ExactName { get; }
        public string JoinLink { get; }
    }
}