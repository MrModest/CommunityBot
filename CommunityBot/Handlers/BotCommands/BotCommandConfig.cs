namespace CommunityBot.Handlers.BotCommands
{
    public class BotCommandConfig
    {
        public BotCommandConfig(string botCommand, bool isForAdmin = false, string? argRequiredMessage = null)
        {
            BotCommand = botCommand;
            IsForAdmin = isForAdmin;
            ArgRequiredMessage = argRequiredMessage;
        }
        
        public BotCommandConfig(string botCommand, string argRequiredMessage)
            : this(botCommand, false, argRequiredMessage)
        {
        }

        public string BotCommand { get; }
        public bool IsForAdmin { get; }
        public string? ArgRequiredMessage { get; }
    }
}