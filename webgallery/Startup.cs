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
            var languageCode = GetLanguageCode(requestContext);
            if (!string.IsNullOrEmpty(languageCode))
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(languageCode);
                }
                catch { }
            }

            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            return DependencyResolver.Current.GetService(controllerType) as IController;
        }

        private static string GetLanguageCode(RequestContext requestContext)
        {
            var languageCode = string.Empty;
            var cookie = requestContext.HttpContext.Request.Cookies["LanguagePreference"];
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                languageCode = cookie.Value;
            }
            else
            {
                var languages = requestContext.HttpContext.Request.UserLanguages;
                if (languages != null && languages.Length > 0)
                {
                    languageCode = languages[0].Contains(";") ? languages[0].Substring(0, languages[0].IndexOf(';')) : languages[0];

                    // the old site use the old language code "zh-chs", "zh-cht" as resource file suffix, and current code of them are "zh-CN" and "zh-TW"
                    switch (languageCode.ToLower())
                    {
                        case "zh-cn": languageCode = "zh-chs"; break;
                        case "zh-tw": languageCode = "zh-cht"; break;
                        default: break;
                    }
                }
            }

            return languageCode;
        }
    }
}