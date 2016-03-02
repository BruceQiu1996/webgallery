using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(webgallery.Startup))]
namespace webgallery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
