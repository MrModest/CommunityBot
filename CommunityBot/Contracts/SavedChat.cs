namespace CommunityBot.Contracts
{
    public class SavedChat
    {
        public SavedChat(long chatId, string exactName, string joinLink)
        {
            ChatId = chatId;
            ExactName = exactName;
            JoinLink = joinLink;
        }

        public long ChatId { get; set; }

        public string ExactName { get; }

        public string JoinLink { get; }
    }
}