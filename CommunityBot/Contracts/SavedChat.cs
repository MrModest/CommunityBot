namespace CommunityBot.Contracts
{
    public class SavedChat
    {
        public SavedChat(long chatId, string customName, string exactName, string joinLink)
        {
            ChatId = chatId;
            CustomName = customName;
            ExactName = exactName;
            JoinLink = joinLink;
        }

        public long ChatId { get; set; }

        public string? CustomName { get; }
        
        public string ExactName { get; }

        public string JoinLink { get; }
    }
}