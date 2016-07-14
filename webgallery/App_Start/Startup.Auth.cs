using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using WebGallery.Security;
using WebGallery.Services;

namespace WebGallery
{
    public partial class Startup
    {
        // For more information on configuring authentication, 
        // please visit https://azure.microsoft.com/en-us/documentation/articles/active-directory-v2-devquickstarts-dotnet-web/
        public void ConfigureAuth(IAppBuilder app)
        {
            var clientId = ConfigurationManager.AppSettings["ida:ClientId"];
            var aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            var redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                // The `Authority` represents the v2.0 endpoint - https://login.microsoftonline.com/common/v2.0
                // The `Scope` describes the permissions that your app will need.  See https://azure.microsoft.com/documentation/articles/active-directory-v2-scopes/
                // In a real application you could use issuer validation for additional checks, like making sure the user's organization has signed up for your app, for instance.

                ClientId = clientId,
                Authority = aadInstance,
                RedirectUri = redirectUri,
                Scope = "openid email profile",
                ResponseType = "id_token",
                PostLogoutRedirectUri = redirectUri,
                TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = OnAuthenticationFailed,
                    SecurityTokenValidated = OnSecurityTokenValidated
                }
            });
        }

        private async Task OnSecurityTokenValidated(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> arg)
        {
            var userIdentity = arg.AuthenticationTicket.Identity as ClaimsIdentity;
            if (userIdentity != null)
            {
                var emailAddress = userIdentity.GetEmailAddress();
                var _submitterService = new SubmitterService();
                var submitter = await _submitterService.GetSubmitterByMicrosoftAccountAsync(emailAddress);
                if (submitter != null)
                {
                    userIdentity.AddClaims(SubmitterClaims.GenerateClaims(submitter));
                }
            }
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            if (notification.Exception.Message.StartsWith("IDX10311"))
            {
                notification.SkipToNextMiddleware();
            }
            else
            {
                notification.Response.Redirect("/error?message=" + notification.Exception.Message);
            }

            return Task.FromResult(0);
        }
    }
}