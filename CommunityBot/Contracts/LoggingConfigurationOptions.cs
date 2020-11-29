namespace CommunityBot.Contracts
{
    public class LoggingConfigurationOptions
    {
        public const string SectionName = "Logging";
        
        public string FilePath { get; set; } = null!;
    }
}