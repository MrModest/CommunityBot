namespace CommunityBot.Contracts
{
    public class BotConfigurationOptions
    {
        public const string SectionName = "BotConfiguration";
        
        public string BotToken { get; set; } = null!;
        
        public string WebhookUrl { get; set; } = null!;

        public string BotName { get; set; } = null!;

        public long MainChannelId { get; set; }
        
        public long MainGroupId { get; set; }
        
        public long ModeratorsChatId { get; set; }
        
        public long[] DebugInfoChatIds { get; set; } = null!;

        public string[] Admins { get; set; } = null!;
        
        public string Version { get; set; } = null!;
    }
}
