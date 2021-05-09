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
            WelcomeMessage = "{ \"IsOn\": false, \"Message\": \"\", \"ButtonName\": \"\", \"ButtonLink\": \"\" }";
        }

        //For Dapper
#pragma warning disable 8618
        public SavedChat()
#pragma warning restore 8618
        {
            
        }

        [Key]
        public long Id { get; set; }

        public long ChatId { get; set; }
        public string ExactName { get; set; }
        public string JoinLink { get; set; }
        
        public string WelcomeMessage { get; set; }
    }

    public record WelcomeMessage (bool IsOn, string Message, string ButtonName, string ButtonLink);
}
