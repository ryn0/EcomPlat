using EcomPlat.Data.Enums;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class ConfigSetting : StateInfo
    {
        public int ConfigSettingId { get; set; }

        public SiteConfigSetting SiteConfigSetting { get; set; }

        public string? Content { get; set; }
    }
}