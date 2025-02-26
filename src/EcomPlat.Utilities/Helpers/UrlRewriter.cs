namespace EcomPlat.Utilities.Helpers
{
    public static class UrlRewriter
    {
        /// <summary>
        /// Rewrites an image URL by replacing the blob prefix with the CDN prefix.
        /// </summary>
        /// <param name="originalUrl">The original URL from the database.</param>
        /// <param name="cdnPrefix">The CDN prefix (with protocol), e.g., "https://cdn.dasjars.com".</param>
        /// <param name="blobPrefix">The Blob prefix (with protocol), e.g., "https://dasjarseastusprod.blob.core.windows.net".</param>
        /// <returns>The rewritten URL using the CDN prefix.</returns>
        public static string RewriteImageUrl(string originalUrl, string cdnPrefix, string blobPrefix)
        {
            if (string.IsNullOrWhiteSpace(originalUrl) ||
                string.IsNullOrWhiteSpace(cdnPrefix) ||
                string.IsNullOrWhiteSpace(blobPrefix))
            {
                return originalUrl;
            }

            // Optionally ensure both prefixes do not have trailing slashes.
            cdnPrefix = cdnPrefix.TrimEnd('/');
            blobPrefix = blobPrefix.TrimEnd('/');

            // If the original URL starts with the blob prefix, replace it with the CDN prefix.
            if (originalUrl.StartsWith(blobPrefix))
            {
                return cdnPrefix + originalUrl.Substring(blobPrefix.Length);
            }

            return originalUrl;
        }
    }
}