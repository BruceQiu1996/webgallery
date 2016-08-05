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
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
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
            else
            {
                var languages = requestContext.HttpContext.Request.UserLanguages;

                // the old site use the old language code "zh-chs", "zh-cht" as resource file suffix, and current code of them are "zh-CN" and "zh-TW"
                if (languages != null && languages.Length > 0)
                {
                    switch (languages[0].ToLower())
                    {
                        case "zh-cn": Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-chs"); break;
                        case "zh-tw": Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-cht"); break;
                        default:
                            try
                            {
                                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(languages[0]);
                            }
                            catch { }
                            break;
                    }
                }
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            }

            return DependencyResolver.Current.GetService(controllerType) as IController;
        }
    }
}