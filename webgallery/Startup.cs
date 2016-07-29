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

            // the old site use the old language code "zh-chs", "zh-cht" as resource file suffix, and current code of them are "zh-CN" and "zh-TW"
            if (Thread.CurrentThread.CurrentCulture.Name.ToLower() == "zh-cn")
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-chs");
            }
            if (Thread.CurrentThread.CurrentCulture.Name.ToLower() == "zh-tw")
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-cht");
            }
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            return DependencyResolver.Current.GetService(controllerType) as IController;
        }
    }
}