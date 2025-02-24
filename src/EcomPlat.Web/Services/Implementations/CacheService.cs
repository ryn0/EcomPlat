using DirectoryManager.Web.Services.Interfaces;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DirectoryManager.Web.Services.Implementations
{
    public class CacheService : ICacheService
    {
        private const string SnippetCachePrefix = "setting-";
        private readonly IConfigSettingRepository configSettingRepository;
        private IMemoryCache memoryCache;

        public CacheService(
            IMemoryCache memoryCache,
            IConfigSettingRepository configSettingRepository)
        {
            this.memoryCache = memoryCache;
            this.configSettingRepository = configSettingRepository;
        }

        public void ClearSnippetCache(SiteConfigSetting snippetType)
        {
            var cacheKey = this.BuildCacheKey(snippetType);

            this.memoryCache.Remove(cacheKey);
        }

        public string GetSnippet(SiteConfigSetting snippetType)
        {
            var cacheKey = this.BuildCacheKey(snippetType);

            if (this.memoryCache.TryGetValue(cacheKey, out string? maybeSnippet) && maybeSnippet != null)
            {
                return maybeSnippet;
            }
            else
            {
                var dbModel = this.configSettingRepository.Get(snippetType);

                var content = dbModel?.Content ?? string.Empty;
                if (dbModel != null)
                {
                    this.memoryCache.Set(cacheKey, content);
                }

                return content;
            }
        }

        private string BuildCacheKey(SiteConfigSetting snippetType)
        {
            return string.Format("{0}{1}", SnippetCachePrefix, snippetType.ToString());
        }
    }
}