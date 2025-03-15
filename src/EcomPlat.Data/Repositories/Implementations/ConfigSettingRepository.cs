using EcomPlat.Data.Constants;
using EcomPlat.Data.DbContextInfo;
using EcomPlat.Data.Enums;
using EcomPlat.Data.Models;
using EcomPlat.Data.Repositories.Interfaces;

namespace EcomPlat.Data.Repositories.Implementations
{
    public class ConfigSettingRepository : IConfigSettingRepository
    {
        public ConfigSettingRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public ConfigSetting Create(ConfigSetting model)
        {
            try
            {
                this.Context.ConfigSettings.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public ConfigSetting? Get(int contentSnippetId)
        {
            try
            {
                var contentSnippet = this.Context.ConfigSettings.Find(contentSnippetId);

                return contentSnippet;
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public ConfigSetting? Get(SiteConfigSetting snippetType)
        {
            try
            {
                var contentSnippet = this.Context.ConfigSettings.FirstOrDefault(x => x.SiteConfigSetting == snippetType);

                return contentSnippet;
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public string GetValue(SiteConfigSetting snippetType)
        {
            try
            {
                var contentSnippet = this.Context.ConfigSettings.FirstOrDefault(x => x.SiteConfigSetting == snippetType);

                if (contentSnippet == null || contentSnippet.Content == null)
                {
                    return string.Empty;
                }

                return contentSnippet.Content;
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(ConfigSetting model)
        {
            try
            {
                this.Context.ConfigSettings.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int contentSnippetId)
        {
            try
            {
                var entry = this.Context.ConfigSettings.Find(contentSnippetId) ??
                    throw new ArgumentNullException($"No ContentSnippet found with ID {contentSnippetId}");
                this.Context.ConfigSettings.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IList<ConfigSetting> GetAll()
        {
            try
            {
                return this.Context.ConfigSettings.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public DateTime? GetLastUpdateDate()
        {
            // Fetch the latest CreateDate and UpdateDate
            var latestCreateDate = this.Context.ConfigSettings
                                   .Where(e => e != null)
                                   .Max(e => (DateTime?)e.CreateDate);

            var latestUpdateDate = this.Context.ConfigSettings
                                   .Where(e => e != null)
                                   .Max(e => e.UpdateDate) ?? DateTime.MinValue;

            // Return the more recent of the two dates
            return (DateTime)(latestCreateDate > latestUpdateDate ? latestCreateDate : latestUpdateDate);
        }
    }
}