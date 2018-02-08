using Microsoft.Owin.Security;
using System.Security.Claims;
using WebGallery.Models;

namespace WebGallery.Security
{
    public static class AuthenticationExtensions
    {
        public static void SignInAsSubmitter(this IAuthenticationManager authMangaer, Submitter submitter)
        {
            var userIdentity = authMangaer.User.Identity as ClaimsIdentity;
            if (userIdentity != null && submitter != null)
            {
                var claims = SubmitterClaims.GenerateClaims(submitter);
                userIdentity.AddClaims(claims);
                authMangaer.SignIn(userIdentity);
            }
        }
    }
}