using System;
using CommunityBot.Contracts;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Services
{
    public class InMemorySettingsService
    {
        private readonly IMemoryCacheWrapper _memoryCache;
        private readonly ILogger<InMemorySettingsService> _logger;

        public InMemorySettingsService(
            IMemoryCacheWrapperFactory memoryCacheWrapperFactory,
            ILogger<InMemorySettingsService> logger)
        {
            _memoryCache = memoryCacheWrapperFactory.CreateWrapper("InMemorySettings_");
            _logger = logger;
        }

        public TValue SetSettingValue<TValue>(InMemorySettingKey settingKey, TValue value)
            where TValue: notnull
        {
            CheckNotNull(value);
            _memoryCache.Set(settingKey.ToString(), value);
            _logger.LogWarning("InMemorySetting {Setting} was changed to {Value}", settingKey, value);
            
            return value;
        }

        public TValue GetSettingValue<TValue>(InMemorySettingKey settingKey, TValue defaultValue)
            where TValue: notnull
        {
            CheckNotNull(defaultValue);
            if (_memoryCache.TryGetValue(settingKey.ToString(), out TValue value))
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
        NewUsersCheck,
        WelcomeMessageSending
    }
}
