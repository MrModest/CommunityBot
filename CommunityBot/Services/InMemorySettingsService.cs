using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Services
{
    public class InMemorySettingsService
    {
        private const string Prefix = "InMemorySettings_";

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemorySettingsService> _logger;

        public InMemorySettingsService(
            IMemoryCache memoryCache,
            ILogger<InMemorySettingsService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public TValue SetSettingValue<TValue>(InMemorySettingKey settingKey, TValue value)
            where TValue: notnull
        {
            CheckNotNull(value);
            _memoryCache.Set($"{Prefix}{settingKey}", value);
            _logger.LogWarning("InMemorySetting {Setting} was changed to {Value}", settingKey, value);
            
            return value;
        }

        public TValue GetSettingValue<TValue>(InMemorySettingKey settingKey, TValue defaultValue)
            where TValue: notnull
        {
            CheckNotNull(defaultValue);
            if (_memoryCache.TryGetValue($"{Prefix}{settingKey}", out TValue value))
            {
                return value;
            }

            SetSettingValue(settingKey, defaultValue);

            return defaultValue;
        }

        private static void CheckNotNull<TValue>(TValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }

    public enum InMemorySettingKey
    {
        CollectUserInfo
    }
}
