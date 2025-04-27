namespace NIA_CRM.Utilities
{
    public static class CookieHelper
    {
        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="_context">the HttpContext</param>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="value">value to store in cookie object</param>  
        /// <param name="expireTime">expiration time</param>  
        public static void CookieSet(HttpContext context, string key, string value, int? expireTime)
        {
            CookieOptions option = new()
            {
                HttpOnly = true,  // Prevent JavaScript access (Security)
                Secure = true,    // Enforce HTTPS
                SameSite = SameSiteMode.Strict // Protects against CSRF
            };

            // Set expiration time if provided, otherwise use session-based cookie
            if (expireTime.HasValue)
                option.Expires = DateTime.UtcNow.AddMinutes(expireTime.Value);

            context.Response.Cookies.Append(key, value, option);
        }

        public static string CookieGet(HttpContext context, string key)
        {
            return context.Request.Cookies.TryGetValue(key, out string value) ? value : null;
        }
    }

}
