using System.Web.Mvc;
using System.Web.Routing;
using WebGallery.Controllers;

namespace WebGallery
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // home
            var home = nameof(HomeController).Replace("Controller", string.Empty);
            routes.MapRoute(name: SiteRouteNames.Home, url: SiteRouteUrlPatterns.Home, defaults: new { controller = home, action = nameof(HomeController.Index) });
            routes.MapRoute(name: SiteRouteNames.Docs, url: SiteRouteUrlPatterns.Docs, defaults: new { controller = home, action = nameof(HomeController.Documents) });
            routes.MapRoute(name: SiteRouteNames.Agreement, url: SiteRouteUrlPatterns.Agreement, defaults: new { controller = home, action = nameof(HomeController.Agreement) });
            routes.MapRoute(name: SiteRouteNames.Language_Select, url: SiteRouteUrlPatterns.Language_Select, defaults: new { controller = home, action = nameof(HomeController.LanguageSelect), languageCode = UrlParameter.Optional });

            // issue
            var issue = nameof(IssueController).Replace("Controller", string.Empty);
            routes.MapRoute(name: SiteRouteNames.Issue_Report, url: SiteRouteUrlPatterns.Issue_Report, defaults: new { controller = issue, action = nameof(IssueController.Report) });

            // error
            var error = nameof(ErrorController).Replace("Controller", string.Empty);
            routes.MapRoute(name: SiteRouteNames.Error, url: SiteRouteUrlPatterns.Error, defaults: new { controller = error, action = nameof(ErrorController.Error) });
            routes.MapRoute(name: SiteRouteNames.Error_404, url: SiteRouteUrlPatterns.Error_404, defaults: new { controller = error, action = nameof(ErrorController.Error404) });
            routes.MapRoute(name: SiteRouteNames.Error_Fire, url: SiteRouteUrlPatterns.Error_Fire, defaults: new { controller = error, action = nameof(ErrorController.Fire) });

            // account
            var account = nameof(AccountController).Replace("Controller", string.Empty);
            routes.MapRoute(name: SiteRouteNames.SignIn, url: SiteRouteUrlPatterns.SignIn, defaults: new { controller = account, action = nameof(AccountController.SignIn) });
            routes.MapRoute(name: SiteRouteNames.SignOut, url: SiteRouteUrlPatterns.SignOut, defaults: new { controller = account, action = nameof(AccountController.SignOut) });
            routes.MapRoute(name: SiteRouteNames.Profile, url: SiteRouteUrlPatterns.Profile, defaults: new { controller = account, action = nameof(AccountController.Me) });
            routes.MapRoute(name: SiteRouteNames.Profile_View, url: SiteRouteUrlPatterns.Profile_View, defaults: new { controller = account, action = nameof(AccountController.View) });

            // app
            var app = nameof(AppController).Replace("Controller", string.Empty);
            var invitation = nameof(InvitationController).Replace("Controller", string.Empty);
            var ownership = nameof(OwnershipController).Replace("Controller", string.Empty);
            routes.MapRoute(name: SiteRouteNames.Gallery, url: SiteRouteUrlPatterns.Gallery, defaults: new { controller = app, action = nameof(AppController.Gallery) });
            routes.MapRoute(name: SiteRouteNames.Portal, url: SiteRouteUrlPatterns.Portal, defaults: new { controller = app, action = nameof(AppController.Mine) });
            routes.MapRoute(name: SiteRouteNames.App_Submit, url: SiteRouteUrlPatterns.App_Submit, defaults: new { controller = app, action = nameof(AppController.New) });
            routes.MapRoute(name: SiteRouteNames.App_Clone, url: SiteRouteUrlPatterns.App_Clone, defaults: new { controller = app, action = nameof(AppController.Clone) });
            routes.MapRoute(name: SiteRouteNames.App_Edit, url: SiteRouteUrlPatterns.App_Edit, defaults: new { controller = app, action = nameof(AppController.Edit) });
            routes.MapRoute(name: SiteRouteNames.App_Verify, url: SiteRouteUrlPatterns.App_Verify, defaults: new { controller = app, action = nameof(AppController.Verify) });
            routes.MapRoute(name: SiteRouteNames.App_Delete, url: SiteRouteUrlPatterns.App_Delete, defaults: new { controller = app, action = nameof(AppController.Delete) });
            routes.MapRoute(name: SiteRouteNames.App_Update_Status, url: SiteRouteUrlPatterns.App_Update_Status, defaults: new { controller = app, action = nameof(AppController.UpdateStatus) });
            routes.MapRoute(name: SiteRouteNames.App_Publish, url: SiteRouteUrlPatterns.App_Publish, defaults: new { controller = app, action = nameof(AppController.Publish) });
            routes.MapRoute(name: SiteRouteNames.App_Categorize, url: SiteRouteUrlPatterns.App_Categorize, defaults: new { controller = app, action = nameof(AppController.Categorize), category = UrlParameter.Optional });
            routes.MapRoute(name: SiteRouteNames.App_Preview, url: SiteRouteUrlPatterns.App_Preview, defaults: new { controller = app, action = nameof(AppController.Preview) });
            routes.MapRoute(name: SiteRouteNames.App_Install, url: SiteRouteUrlPatterns.App_Install, defaults: new { controller = app, action = nameof(AppController.Install) });
            routes.MapRoute(name: SiteRouteNames.App_Owners, url: SiteRouteUrlPatterns.App_Owners, defaults: new { controller = app, action = nameof(AppController.Owners) });
            routes.MapRoute(name: SiteRouteNames.App_Owners_Invite, url: SiteRouteUrlPatterns.App_Owners_Invite, defaults: new { controller = invitation, action = nameof(InvitationController.Send) });
            routes.MapRoute(name: SiteRouteNames.App_Owners_Remove, url: SiteRouteUrlPatterns.App_Owners_Remove, defaults: new { controller = ownership, action = nameof(OwnershipController.Remove) });
            routes.MapRoute(name: SiteRouteNames.App_Issues_Report, url: SiteRouteUrlPatterns.App_Issues_Report, defaults: new { controller = issue, action = nameof(IssueController.Report) });
            routes.MapRoute(name: SiteRouteNames.App_View, url: SiteRouteUrlPatterns.App_View, defaults: new { controller = app, action = nameof(AppController.ViewFromFeed) });

            // invitation
            routes.MapRoute(name: SiteRouteNames.Invitation_Revoke, url: SiteRouteUrlPatterns.Invitation_Revoke, defaults: new { controller = invitation, action = nameof(InvitationController.Revoke) });
            routes.MapRoute(name: SiteRouteNames.Invitation_Accept, url: SiteRouteUrlPatterns.Invitation_Accept, defaults: new { controller = invitation, action = nameof(InvitationController.Accept) });
            routes.MapRoute(name: SiteRouteNames.Invitation_Decline, url: SiteRouteUrlPatterns.Invitation_Decline, defaults: new { controller = invitation, action = nameof(InvitationController.Decline) });
            routes.MapRoute(name: SiteRouteNames.Invitation_Detail, url: SiteRouteUrlPatterns.Invitation_Detail, defaults: new { controller = invitation, action = nameof(InvitationController.Detail) });

            // admin/manage
            var manage = nameof(ManageController).Replace("Controller", string.Empty);
            routes.MapRoute(name: SiteRouteNames.Dashboard, url: SiteRouteUrlPatterns.Dashboard, defaults: new { controller = manage, action = nameof(ManageController.Dashboard) });
            routes.MapRoute(name: SiteRouteNames.Supersubmitter, url: SiteRouteUrlPatterns.Supersubmitter, defaults: new { controller = manage, action = nameof(ManageController.SuperSubmitters) });
            routes.MapRoute(name: SiteRouteNames.Supersubmitter_Add, url: SiteRouteUrlPatterns.Supersubmitter_Add, defaults: new { controller = manage, action = nameof(ManageController.AddSuperSubmitter) });
            routes.MapRoute(name: SiteRouteNames.Supersubmitter_Remove, url: SiteRouteUrlPatterns.Supersubmitter_Remove, defaults: new { controller = manage, action = nameof(ManageController.RemoveSuperSubmitter) });
            routes.MapRoute(name: SiteRouteNames.Published_Apps, url: SiteRouteUrlPatterns.Published_Apps, defaults: new { controller = manage, action = nameof(ManageController.PublishedApps) });

            // for ajax requests
            routes.MapRoute(name: SiteRouteNames.App_Url_Verify, url: SiteRouteUrlPatterns.App_Url_Verify, defaults: new { controller = app, action = nameof(AppController.VerifyUrl) });
            routes.MapRoute(name: SiteRouteNames.App_Package_Verify, url: SiteRouteUrlPatterns.App_Package_Verify, defaults: new { controller = app, action = nameof(AppController.VerifyPackage) });
            routes.MapRoute(name: SiteRouteNames.App_Image_Verify, url: SiteRouteUrlPatterns.App_Image_Verify, defaults: new { controller = app, action = nameof(AppController.VerifyImage) });
            routes.MapRoute(name: SiteRouteNames.App_Nickname_Version_Validate, url: SiteRouteUrlPatterns.App_Nickname_Version_Validate, defaults: new { controller = app, action = nameof(AppController.ValidateAppIdVersion) });
        }
    }

    public class SiteRouteUrlPatterns
    {
        // home
        public const string Home = "";
        public const string Docs = "docs";
        public const string Agreement = "agreement";

        // language
        public const string Language_Select = "language/{languageCode}";

        // issue
        public const string Issue_Report = "issues/report";

        // error
        public const string Error = "error";
        public const string Error_404 = "errors/404";
        public const string Error_Fire = "errors/fire";

        // account
        public const string SignIn = "signin";
        public const string SignOut = "signout";
        public const string Profile = "profile";
        public const string Profile_View = "profiles/{submitterId}";

        // app
        public const string Gallery = "gallery";
        public const string Portal = "portal";
        public const string App_Submit = "apps/submit";
        public const string App_Clone = "apps/{submissionId}/clone";
        public const string App_Edit = "apps/{submissionId}/edit";
        public const string App_Verify = "apps/{submissionId}/verify";
        public const string App_Delete = "apps/{submissionId}/delete";
        public const string App_Update_Status = "apps/{submissionId}/status/update";
        public const string App_Publish = "apps/{submissionId}/publish";
        public const string App_Categorize = "apps/categorize/{category}";
        public const string App_Preview = "apps/{submissionId}/preview";
        public const string App_Install = "apps/{appId}/install";
        public const string App_Owners = "apps/{submissionId}/owners";
        public const string App_Owners_Invite = "apps/{submissionId}/owners/invite";
        public const string App_Owners_Remove = "apps/{submissionId}/owners/remove/{submitterId}";
        public const string App_Issues_Report = "apps/{appId}/issues/report";
        public const string App_View = "apps/{appId}";

        // invitation
        public const string Invitation_Revoke = "ownership/invitations/{invitationGuid}/revoke";
        public const string Invitation_Accept = "ownership/invitations/{invitationGuid}/accept";
        public const string Invitation_Decline = "ownership/invitations/{invitationGuid}/decline";
        public const string Invitation_Detail = "ownership/invitations/{invitationGuid}";

        // admin/manage
        public const string Dashboard = "admin/dashboard";
        public const string Supersubmitter = "admin/supersubmitters";
        public const string Supersubmitter_Add = "admin/supersubmitters/add";
        public const string Supersubmitter_Remove = "admin/supersubmitters/remove";
        public const string Published_Apps = "admin/apps/published";

        // for ajax requests
        public const string App_Url_Verify = "app/urls/verify";
        public const string App_Package_Verify = "app/packages/verify";
        public const string App_Image_Verify = "app/images/verify/";
        public const string App_Nickname_Version_Validate = "app/nickname/version/validate";
    }

    public class SiteRouteNames
    {
        // home
        public const string Home = nameof(Home);
        public const string Docs = nameof(Docs);
        public const string Agreement = nameof(Agreement);

        // language
        public const string Language_Select = nameof(Language_Select);

        // issue
        public const string Issue_Report = nameof(Issue_Report);

        // error
        public const string Error = nameof(Error);
        public const string Error_404 = nameof(Error_404);
        public const string Error_Fire = nameof(Error_Fire);

        // account
        public const string SignIn = nameof(SignIn);
        public const string SignOut = nameof(SignOut);
        public const string Profile = nameof(Profile);
        public const string Profile_View = nameof(Profile_View);

        // app
        public const string Gallery = nameof(Gallery);
        public const string Portal = nameof(Portal);
        public const string App_Submit = nameof(App_Submit);
        public const string App_Clone = nameof(App_Clone);
        public const string App_Edit = nameof(App_Edit);
        public const string App_Verify = nameof(App_Verify);
        public const string App_Delete = nameof(App_Delete);
        public const string App_Update_Status = nameof(App_Update_Status);
        public const string App_Publish = nameof(App_Publish);
        public const string App_Categorize = nameof(App_Categorize);
        public const string App_Preview = nameof(App_Preview);
        public const string App_Install = nameof(App_Install);
        public const string App_Owners = nameof(App_Owners);
        public const string App_Owners_Invite = nameof(App_Owners_Invite);
        public const string App_Owners_Remove = nameof(App_Owners_Remove);
        public const string App_Issues_Report = nameof(App_Issues_Report);
        public const string App_View = nameof(App_View);

        // inviation
        public const string Invitation_Revoke = nameof(Invitation_Revoke);
        public const string Invitation_Accept = nameof(Invitation_Accept);
        public const string Invitation_Decline = nameof(Invitation_Decline);
        public const string Invitation_Detail = nameof(Invitation_Detail);

        // admin/manage
        public const string Dashboard = nameof(Dashboard);
        public const string Supersubmitter = nameof(Supersubmitter);
        public const string Supersubmitter_Add = nameof(Supersubmitter_Add);
        public const string Supersubmitter_Remove = nameof(Supersubmitter_Remove);
        public const string Published_Apps = nameof(Published_Apps);

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