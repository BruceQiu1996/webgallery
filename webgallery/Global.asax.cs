using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebGallery.Services;

namespace WebGallery
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            // if the logos-screenshots container doesn't exist, create it.
            new AppImageAzureStorageService().CreateContainerIfNotExists();

            // use a culture-aware controller activator to set culture for current thread
            ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory(new CultureAwareControllerActivator()));
        }
    }
}