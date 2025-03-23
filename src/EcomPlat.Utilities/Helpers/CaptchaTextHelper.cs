namespace EcomPlat.Utilities.Helpers
{
    public class CaptchaTextHelper
    {
        /// <summary>
        /// Helper method to generate a random CAPTCHA string
        /// </summary>
        /// <param name="length">Character length.</param>
        /// <returns>The captcha text.</returns>
        public static string GenerateCaptchaText(int length = 5)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}