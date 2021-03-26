namespace CommunityBot.Contracts
{
    public class LoggingConfigurationOptions
    {
        public const string SectionName = "Logging";
        public string LogDir { get; set; } = null!;
    }
}