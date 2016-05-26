using System.Security.Claims;
using System.Security.Principal;
using WebGallery.Extensions;
using WebGallery.Models;

namespace WebGallery.Security
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

        public static string GetNameIdentifier(this IPrincipal user)
        {
            return ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
        }

        public static int GetSubmitterId(this IPrincipal user)
        {
            return ClaimsPrincipal.Current.FindFirst(SubmitterClaimsType.SubmitterId).Value.ToInt();
        }

        public static bool IsSuperSubmitter(this IPrincipal user)
        {
            return ClaimsPrincipal.Current.FindFirst(SubmitterClaimsType.IsSuperSubmitter).Value.ToBool();
        }

        public static string GetMicrosoftAccount(this IPrincipal user)
        {
            return ClaimsPrincipal.Current.FindFirst(SubmitterClaimsType.MicrosoftAccount).Value;
        }

        public static Submitter GetSubmittership(this IPrincipal user)
        {
            return new Submitter
            {
                SubmitterID = user.GetSubmitterId(),
                IsSuperSubmitter = user.IsSuperSubmitter(),
                MicrosoftAccount = user.GetMicrosoftAccount()
            };
        }
    }
}