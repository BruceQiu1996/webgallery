using Microsoft.Owin;
using Owin;
using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartupAttribute(typeof(WebGallery.Startup))]
namespace WebGallery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }

    public class CultureAwareControllerActivator : IControllerActivator
    {
        public IController Create(RequestContext requestContext, Type controllerType)
        {
            var cookie = requestContext.HttpContext.Request.Cookies["LanguagePreference"];
            if (cookie != null)
            {
                try
                {
                    var culture = CultureInfo.GetCultureInfo(cookie.Value);
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                }

                // If the cookie.Value can't be used to get a CultureInfo, we keep the CurrentCulture unchanged
                catch { }
            }

            return DependencyResolver.Current.GetService(controllerType) as IController;
        }
    }
}