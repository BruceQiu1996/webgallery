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

            bundles.Add(new StyleBundle("~/Content/site").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/_site.css"));

            #region home ----------------------------------------------
            // home.index
            bundles.Add(new StyleBundle("~/css/home-index").Include(
                      "~/Content/_home-index.css"));

            // home.agreement
            bundles.Add(new StyleBundle("~/css/home-agreement").Include(
                      "~/Content/_home-agreement.css"));
            #endregion

            #region app ----------------------------------------------
            // app.mine
            bundles.Add(new ScriptBundle("~/js/app-mine").Include(
                                    "~/scripts/_app-mine.js",
                                    "~/scripts/moment.*",
                                    "~/scripts/bootstrap-sortable.*"));
            bundles.Add(new StyleBundle("~/css/app-mine").Include(
                      "~/Content/_app-mine.css",
                      "~/Content/bootstrap-sortable.css"));

            // app.submit
            bundles.Add(new ScriptBundle("~/js/app_submit").Include(
                        "~/scripts/_app_submit.js",
                        "~/scripts/bootstrap-datepicker*"));
            bundles.Add(new StyleBundle("~/css/app_submit").Include(
                      "~/Content/_app_submit.css",
                      "~/Content/_tab.css",
                      "~/Content/bootstrap-datepicker3*"));

            // app.verify
            bundles.Add(new ScriptBundle("~/js/app-verify").Include(
                        "~/scripts/_app-verify.js"));
            bundles.Add(new StyleBundle("~/css/app-verify").Include(
                      "~/Content/_app-verify.css"));

            // app.preview
            bundles.Add(new StyleBundle("~/css/app-preview").Include(
                "~/Content/_app-preview.css"));
            bundles.Add(new ScriptBundle("~/js/app-preview").Include(
                        "~/scripts/_app-preview.js"));

            // app.gallery
            bundles.Add(new StyleBundle("~/css/app-gallery").Include(
                "~/Content/_app-gallery.css"));
            bundles.Add(new ScriptBundle("~/js/app-gallery").Include(
                        "~/scripts/_app-gallery.js"));

            // app.categorize
            bundles.Add(new StyleBundle("~/css/app-categorize").Include(
                "~/Content/_app-categorize.css"));
            bundles.Add(new ScriptBundle("~/js/app-categorize").Include(
                        "~/scripts/_app-categorize.js"));

            // app.install
            bundles.Add(new StyleBundle("~/css/app-install").Include(
                "~/Content/_app-install.css"));

            // app.owners
            bundles.Add(new ScriptBundle("~/js/app-owners").Include(
                        "~/scripts/_app-owners.js"));
            bundles.Add(new StyleBundle("~/css/app-owners").Include(
                      "~/Content/_app-owners.css"));

            // app.publish
            bundles.Add(new StyleBundle("~/css/app-publish").Include(
               "~/Content/_app-publish.css"));
            #endregion

            #region issue ----------------------------------------------
            // issue.report
            bundles.Add(new StyleBundle("~/css/issue-report").Include(
                      "~/Content/_issue-report.css"));
            #endregion

            #region invitation ----------------------------------------------
            // invitation.send
            bundles.Add(new StyleBundle("~/css/invitation-send").Include(
                      "~/Content/_invitation-send.css"));

            // invitation.detail
            bundles.Add(new StyleBundle("~/css/invitation-detail").Include(
                      "~/Content/_invitation-detail.css"));
            #endregion

            #region account ----------------------------------------------
            // account.view
            bundles.Add(new StyleBundle("~/css/account-view").Include(
                "~/Content/_account-view.css"));

            // account.me
            bundles.Add(new ScriptBundle("~/js/account-me").Include(
                        "~/scripts/_account-me.js"));
            bundles.Add(new StyleBundle("~/css/account-me").Include(
                "~/Content/_account-me.css"));
            #endregion

            #region manage ----------------------------------------------
            // manage.dashboard
            bundles.Add(new StyleBundle("~/css/manage-dashboard").Include(
                "~/Content/_manage-dashboard.css"));
            bundles.Add(new ScriptBundle("~/js/manage-dashboard").Include(
                        "~/scripts/_manage-dashboard.js"));

            // manage.supersubmitters
            bundles.Add(new StyleBundle("~/css/super-submitters").Include(
                "~/Content/_super-submitters.css"));
            bundles.Add(new ScriptBundle("~/js/super-submitters").Include(
                        "~/scripts/_super-submitters.js"));

            // manage.feeds
            bundles.Add(new StyleBundle("~/css/manage_feeds").Include(
                "~/Content/_manage_feeds.css"));
            bundles.Add(new ScriptBundle("~/js/manage_feeds").Include(
                       "~/scripts/_manage_feeds.js"));
            #endregion
        }
    }
}