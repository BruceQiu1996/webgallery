using System.Web.Mvc;
using System.Web.Routing;

namespace WebGallery
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(name: SiteRouteNames.Home, url: SiteRouteUrlPatterns.Home, defaults: new { controller = "Home", action = "Index" });
            routes.MapRoute(name: SiteRouteNames.Gallery, url: SiteRouteUrlPatterns.Gallery, defaults: new { controller = "App", action = "Gallery" });
            routes.MapRoute(name: SiteRouteNames.Docs, url: SiteRouteUrlPatterns.Docs, defaults: new { controller = "Home", action = "Documentation" });
            routes.MapRoute(name: SiteRouteNames.Issue_Report, url: SiteRouteUrlPatterns.Issue_Report, defaults: new { controller = "Home", action = "ReportIssue" });
            routes.MapRoute(name: SiteRouteNames.Agreement, url: SiteRouteUrlPatterns.Agreement, defaults: new { controller = "Home", action = "Agreement" });
            routes.MapRoute(name: SiteRouteNames.Error, url: SiteRouteUrlPatterns.Error, defaults: new { controller = "Home", action = "Error" });

            routes.MapRoute(name: SiteRouteNames.SignIn, url: SiteRouteUrlPatterns.SignIn, defaults: new { controller = "Account", action = "SignIn" });
            routes.MapRoute(name: SiteRouteNames.SignOut, url: SiteRouteUrlPatterns.SignOut, defaults: new { controller = "Account", action = "SignOut" });
            routes.MapRoute(name: SiteRouteNames.Profile, url: SiteRouteUrlPatterns.Profile, defaults: new { controller = "Account", action = "Me" });
            routes.MapRoute(name: SiteRouteNames.Profile_View, url: SiteRouteUrlPatterns.Profile_View, defaults: new { controller = "Account", action = "View" });

            routes.MapRoute(name: SiteRouteNames.Portal, url: SiteRouteUrlPatterns.Portal, defaults: new { controller = "App", action = "Mine" });
            routes.MapRoute(name: SiteRouteNames.App_Submit, url: SiteRouteUrlPatterns.App_Submit, defaults: new { controller = "App", action = "new" });
            routes.MapRoute(name: SiteRouteNames.App_Clone, url: SiteRouteUrlPatterns.App_Clone, defaults: new { controller = "App", action = "Clone" });
            routes.MapRoute(name: SiteRouteNames.App_Edit, url: SiteRouteUrlPatterns.App_Edit, defaults: new { controller = "App", action = "Edit" });
            routes.MapRoute(name: SiteRouteNames.App_Verify, url: SiteRouteUrlPatterns.App_Verify, defaults: new { controller = "App", action = "Verify" });
            routes.MapRoute(name: SiteRouteNames.App_Delete, url: SiteRouteUrlPatterns.App_Delete, defaults: new { controller = "App", action = "Delete" });
            routes.MapRoute(name: SiteRouteNames.App_Update_Status, url: SiteRouteUrlPatterns.App_Update_Status, defaults: new { controller = "App", action = "UpdateStatus" });
            routes.MapRoute(name: SiteRouteNames.App_Categorize, url: SiteRouteUrlPatterns.App_Categorize, defaults: new { controller = "App", action = "Categorize" });
            routes.MapRoute(name: SiteRouteNames.App_Preview, url: SiteRouteUrlPatterns.App_Preview, defaults: new { controller = "App", action = "Preview" });
            routes.MapRoute(name: SiteRouteNames.App_Install, url: SiteRouteUrlPatterns.App_Install, defaults: new { controller = "App", action = "Install" });
            routes.MapRoute(name: SiteRouteNames.App_Owners, url: SiteRouteUrlPatterns.App_Owners, defaults: new { controller = "App", action = "Owners" });
            routes.MapRoute(name: SiteRouteNames.App_Owners_Invite, url: SiteRouteUrlPatterns.App_Owners_Invite, defaults: new { controller = "Invitation", action = "Send" });
            routes.MapRoute(name: SiteRouteNames.App_Owners_Remove, url: SiteRouteUrlPatterns.App_Owners_Remove, defaults: new { controller = "Ownership", action = "Remove" });
            routes.MapRoute(name: SiteRouteNames.App_View, url: SiteRouteUrlPatterns.App_View, defaults: new { controller = "App", action = "Preview" });

            routes.MapRoute(name: SiteRouteNames.Invitation_Revoke, url: SiteRouteUrlPatterns.Invitation_Revoke, defaults: new { controller = "Invitation", action = "Revoke" });
            routes.MapRoute(name: SiteRouteNames.Invitation_Accept, url: SiteRouteUrlPatterns.Invitation_Accept, defaults: new { controller = "Invitation", action = "Accept" });
            routes.MapRoute(name: SiteRouteNames.Invitation_Decline, url: SiteRouteUrlPatterns.Invitation_Decline, defaults: new { controller = "Invitation", action = "Decline" });
            routes.MapRoute(name: SiteRouteNames.Invitation_Detail, url: SiteRouteUrlPatterns.Invitation_Detail, defaults: new { controller = "Invitation", action = "Detail" });

            routes.MapRoute(name: SiteRouteNames.Dashboard, url: SiteRouteUrlPatterns.Dashboard, defaults: new { controller = "Manage", action = "Dashboard" });
            routes.MapRoute(name: SiteRouteNames.Supersubmitter, url: SiteRouteUrlPatterns.Supersubmitter, defaults: new { controller = "Manage", action = "Supersubmitters" });

            // for ajax requests
            routes.MapRoute(name: SiteRouteNames.App_Url_Verify, url: SiteRouteUrlPatterns.App_Url_Verify, defaults: new { controller = "App", action = "VerifyUrl" });
            routes.MapRoute(name: SiteRouteNames.App_Package_Verify, url: SiteRouteUrlPatterns.App_Package_Verify, defaults: new { controller = "App", action = "VerifyPackage" });
            routes.MapRoute(name: SiteRouteNames.App_Image_Verify, url: SiteRouteUrlPatterns.App_Image_Verify, defaults: new { controller = "App", action = "VerifyImage" });
            routes.MapRoute(name: SiteRouteNames.App_Nickname_Version_Validate, url: SiteRouteUrlPatterns.App_Nickname_Version_Validate, defaults: new { controller = "App", action = "ValidateAppIdVersion" });
            // default route
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
    public class SiteRouteUrlPatterns
    {
        public const string Home = "";
        public const string Gallery = "gallery";
        public const string Docs = "docs";
        public const string Issue_Report = "issues/report";
        public const string Agreement = "agreement";
        public const string Error = "error";

        public const string SignIn = "signin";
        public const string SignOut = "signout";
        public const string Profile = "profile";
        public const string Profile_View = "profiles/{submitterId}";

        public const string Portal = "portal";
        public const string App_Submit = "apps/submit";
        public const string App_Clone = "apps/{submissionId}/clone";
        public const string App_Edit = "apps/{submissionId}/edit";
        public const string App_Verify = "apps/{submissionId}/verify";
        public const string App_Delete = "apps/{submissionId}/delete";
        public const string App_Update_Status = "apps/{submissionId}/status/update";
        public const string App_Categorize = "apps/categorize/{category}";
        public const string App_Preview = "apps/{submissionId}/preview";
        public const string App_Install = "apps/{appId}/install";
        public const string App_Owners = "apps/{submissionId}/owners";
        public const string App_Owners_Invite = "apps/{submissionId}/owners/invite";
        public const string App_Owners_Remove = "apps/{submissionId}/owners/remove/{submitterId}";
        public const string App_View = "apps/{appId}";

        public const string Invitation_Revoke = "ownership/invitations/{invitationGuid}/revoke";
        public const string Invitation_Accept = "ownership/invitations/{invitationGuid}/accept";
        public const string Invitation_Decline = "ownership/invitations/{invitationGuid}/decline";
        public const string Invitation_Detail = "ownership/invitations/{invitationGuid}";

        public const string Dashboard = "admin/dashboard";
        public const string Supersubmitter = "admin/supersubmitters";

        // for ajax requests
        public const string App_Url_Verify = "app/urls/verify";
        public const string App_Package_Verify = "app/packages/verify";
        public const string App_Image_Verify = "app/images/verify/";
        public const string App_Nickname_Version_Validate = "app/nickname/version/validate";
    }

    public class SiteRouteNames
    {
        public const string Home = nameof(Home);
        public const string Gallery = nameof(Gallery);
        public const string Docs = nameof(Docs);
        public const string Issue_Report = nameof(Issue_Report);
        public const string Agreement = nameof(Agreement);
        public const string Error = nameof(Error);

        public const string SignIn = nameof(SignIn);
        public const string SignOut = nameof(SignOut);
        public const string Profile = nameof(Profile);
        public const string Profile_View = nameof(Profile_View);

        public const string Portal = nameof(Portal);
        public const string App_Submit = nameof(App_Submit);
        public const string App_Clone = nameof(App_Clone);
        public const string App_Edit = nameof(App_Edit);
        public const string App_Verify = nameof(App_Verify);
        public const string App_Delete = nameof(App_Delete);
        public const string App_Update_Status = nameof(App_Update_Status);
        public const string App_Categorize = nameof(App_Categorize);
        public const string App_Preview = nameof(App_Preview);
        public const string App_Install = nameof(App_Install);
        public const string App_Owners = nameof(App_Owners);
        public const string App_Owners_Invite = nameof(App_Owners_Invite);
        public const string App_Owners_Remove = nameof(App_Owners_Remove);
        public const string App_View = nameof(App_View);

        public const string Invitation_Revoke = nameof(Invitation_Revoke);
        public const string Invitation_Accept = nameof(Invitation_Accept);
        public const string Invitation_Decline = nameof(Invitation_Decline);
        public const string Invitation_Detail = nameof(Invitation_Detail);

        public const string Dashboard = nameof(Dashboard);
        public const string Supersubmitter = nameof(Supersubmitter);

        // for ajax requests
        public const string App_Url_Verify = nameof(App_Url_Verify);
        public const string App_Package_Verify = nameof(App_Package_Verify);
        public const string App_Image_Verify = nameof(App_Image_Verify);
        public const string App_Nickname_Version_Validate = nameof(App_Nickname_Version_Validate);
    }

    public class SiteUrls
    {
        public const string Home = Root;
        public const string Portal = Root + SiteRouteUrlPatterns.Portal;
        private const string Root = "/";
    }
}