using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;

namespace EcomPlat.Data.Repositories.Interfaces
{
    public interface IConfigSettingRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        ConfigSetting Create(ConfigSetting model);

        bool Update(ConfigSetting model);

        ConfigSetting? Get(int configSettingId);

        DateTime? GetLastUpdateDate();

        ConfigSetting? Get(SiteConfigSetting snippetType);

        string GetValue(SiteConfigSetting snippetType);

        bool Delete(int tagId);

        IList<ConfigSetting> GetAll();
    }
}