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
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(requestContext.HttpContext.Request.Cookies["LanguagePreference"].Value);
            }

            // If the cookie is null or value of cookie can't be used to get a CultureInfo, the user language of request should be used
            catch
            {
                var languages = requestContext.HttpContext.Request.UserLanguages;
                if (languages != null && languages.Length > 0)
                {
                    var lang = languages[0].Contains(";") ? languages[0].Substring(0, languages[0].IndexOf(';')) : languages[0];

                    // the old site use the old language code "zh-chs", "zh-cht" as resource file suffix, and current code of them are "zh-CN" and "zh-TW"
                    switch (lang.ToLower())
                    {
                        case "zh-cn": lang = "zh-chs"; break;
                        case "zh-tw": lang = "zh-cht"; break;
                        default: break;
                    }

                    try
                    {
                        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(lang);
                    }
                    catch { }
                }
            }

            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            return DependencyResolver.Current.GetService(controllerType) as IController;
        }
    }
}