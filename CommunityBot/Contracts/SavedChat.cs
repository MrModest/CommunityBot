namespace CommunityBot.Contracts
{
    [Dapper.Contrib.Extensions.Table("SavedChats")]
    public class SavedChat : EntityBase
    {
        public SavedChat(long chatId, string exactName, string joinLink)
        {
            Id = chatId;
            ExactName = exactName;
            JoinLink = joinLink;
        }

        public SavedChat()
        {
            
        }

        public string ExactName { get; }
        public string JoinLink { get; }
    }
}