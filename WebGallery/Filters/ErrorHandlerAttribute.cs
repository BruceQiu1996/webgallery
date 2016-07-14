using System.Web.Mvc;

namespace WebGallery.Filters
{
    public class ErrorHandlerAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;

                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

                return;
            }

            base.OnException(filterContext);
        }
    }
}