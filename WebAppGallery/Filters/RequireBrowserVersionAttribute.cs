using System.Web.Mvc;
using WebGallery.Extensions;

namespace WebGallery.Filters
{
    public class RequireBrowserVersionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if the browser is IE and its version is less than 10.
            // If yes, then the user will be prompted to upgrade IE because the page uses placeholder (a HTML 5 attribute) to show watermark text.
            // See http://www.w3schools.com/tags/att_input_placeholder.asp.
            if (filterContext.HttpContext.Request.Browser.IsInternetExplorer() && filterContext.HttpContext.Request.Browser.MajorVersion < 10)
            {
                filterContext.Result = new ViewResult { ViewName = "UpgradeIE" };
            }

            base.OnActionExecuting(filterContext);
        }
    }
}