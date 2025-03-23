namespace EcomPlat.Web.Converters
{
    public static class UrlConverters
    {
        public static (string cdnPrefix, string blobPrefix) ConvertCdnUrls(string cdnSetting, string blobSetting)
        {
            string cdnPrefix = cdnSetting?.TrimEnd('/') ?? string.Empty;
            string blobPrefix = blobSetting?.TrimEnd('/') ?? string.Empty;
            return (cdnPrefix, blobPrefix);
        }
    }
}
