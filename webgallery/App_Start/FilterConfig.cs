using System.Web.Mvc;
using WebGallery.Filters;

namespace WebGallery
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorHandlerAttribute());
            filters.Add(new RequireHttpsAttribute());
        }
    }
}