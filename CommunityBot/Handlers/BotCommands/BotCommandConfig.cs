namespace CommunityBot.Handlers.BotCommands
{
    public class BotCommandConfig
    {
        public BotCommandConfig(string botCommand, bool isForAdmin = false, bool allowOnlyInPrivate = false, string? argRequiredMessage = null)
        {
            BotCommand = botCommand;
            IsForAdmin = isForAdmin;
            AllowOnlyInPrivate = allowOnlyInPrivate;
            ArgRequiredMessage = argRequiredMessage;
        }
        
        public BotCommandConfig(string botCommand, string argRequiredMessage)
            : this(botCommand, false, false, argRequiredMessage)
        {
        }

        public string BotCommand { get; }
        public bool IsForAdmin { get; }
        
        public bool AllowOnlyInPrivate { get; }
        
        public string? ArgRequiredMessage { get; }
    }
}