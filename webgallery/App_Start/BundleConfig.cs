using System.Web.Optimization;

namespace WebGallery
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.*",
                        "~/Scripts/jquery.validate.unobtrusive.*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/_site.css"));

            bundles.Add(new StyleBundle("~/css/home-index").Include(
                      "~/Content/_home-index.css"));

            bundles.Add(new ScriptBundle("~/js/submission-form").Include(
                        "~/scripts/_submission-form.js",
                        "~/scripts/bootstrap-datepicker*"));
            bundles.Add(new StyleBundle("~/css/submission-form").Include(
                      "~/Content/_submission-form.css",
                      "~/Content/_tab.css",
                      "~/Content/bootstrap-datepicker3*"));

            bundles.Add(new ScriptBundle("~/js/app-verify").Include(
                        "~/scripts/_app-verify.js"));
            bundles.Add(new StyleBundle("~/css/app-verify").Include(
                      "~/Content/_app-verify.css"));

            bundles.Add(new ScriptBundle("~/js/app-mine").Include(
                                    "~/scripts/_app-mine.js",
                                    "~/scripts/moment.*",
                                    "~/scripts/bootstrap-sortable.*"));
            bundles.Add(new StyleBundle("~/css/app-mine").Include(
                      "~/Content/_app-mine.css",
                      "~/Content/bootstrap-sortable.css"));

            bundles.Add(new ScriptBundle("~/js/app-owners").Include(
                        "~/scripts/_app-owners.js"));
            bundles.Add(new StyleBundle("~/css/app-owners").Include(
                      "~/Content/_app-owners.css"));

            bundles.Add(new StyleBundle("~/css/invitation-send").Include(
                      "~/Content/_invitation-send.css"));
            bundles.Add(new StyleBundle("~/css/invitation-detail").Include(
                      "~/Content/_invitation-detail.css"));

            bundles.Add(new StyleBundle("~/css/account-view").Include(
                "~/Content/_account-view.css"));

            bundles.Add(new ScriptBundle("~/js/account-profile-form").Include(
                        "~/scripts/_account-profile.js"));
            bundles.Add(new StyleBundle("~/css/account-profile-form").Include(
                "~/Content/_account-profile.css"));

            bundles.Add(new StyleBundle("~/css/app-detail").Include(
                "~/Content/_app-detail.css"));
            bundles.Add(new ScriptBundle("~/js/app-detail").Include(
                        "~/scripts/_app-detail.js"));

            bundles.Add(new StyleBundle("~/css/app-gallery").Include(
                "~/Content/_app-gallery.css"));
            bundles.Add(new ScriptBundle("~/js/app-gallery").Include(
                        "~/scripts/_app-gallery.js"));

            bundles.Add(new StyleBundle("~/css/manage-dashboard").Include(
                "~/Content/_manage-dashboard.css"));
            bundles.Add(new ScriptBundle("~/js/manage-dashboard").Include(
                        "~/scripts/_manage-dashboard.js"));

            bundles.Add(new StyleBundle("~/css/app-install").Include(
                "~/Content/_app-install.css"));
        }
    }
}