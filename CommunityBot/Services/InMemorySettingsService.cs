using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Services
{
    public class InMemorySettingsService
    {
        private const string Prefix = "InMemorySettings_";

        private const string CollectUserInfoSettingKey = "CollectUserInfo";
        
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemorySettingsService> _logger;

        public InMemorySettingsService(
            IMemoryCache memoryCache,
            ILogger<InMemorySettingsService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public void SetSettingCollectUserInfo(bool value)
        {
            _memoryCache.Set(Prefix + CollectUserInfoSettingKey, value);
            _logger.LogWarning("InMemorySetting {setting} was changed to {value}", CollectUserInfoSettingKey, value);
        }

        public bool GetSettingCollectUserInfo()
        {
            if (_memoryCache.TryGetValue(Prefix + CollectUserInfoSettingKey, out bool value))
            {
                return value;
            }
            
            SetSettingCollectUserInfo(false);

            return false;
        }
    }
}