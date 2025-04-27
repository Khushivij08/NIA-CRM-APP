using System.Text.RegularExpressions;

namespace NIA_CRM.Utilities
{

    public static class TextHelper
    {
        public static string TruncateHtmlText(string html, int maxLength)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Remove HTML tags
            string plainText = Regex.Replace(html, "<.*?>", string.Empty);

            // Truncate
            return plainText.Length > maxLength ? plainText.Substring(0, maxLength) + "..." : plainText;
        }
    }

}
