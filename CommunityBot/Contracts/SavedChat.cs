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
#pragma warning disable 8618
        public SavedChat()
#pragma warning restore 8618
        {
            
        }

        public long ChatId { get; }
        public string ExactName { get; }
        public string JoinLink { get; }
    }
}