using System.Security.Claims;
using System.Security.Principal;

namespace WebGallery.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetName(this IPrincipal user)
        {
            return ClaimsPrincipal.Current.FindFirst("name").Value;
        }

        public static string GetPreferredUsername(this IPrincipal user)
        {
            return ClaimsPrincipal.Current.FindFirst("preferred_username").Value;
        }

        public static string GetEmailAddress(this IPrincipal user)
        {
            return ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
        }
    }
}