using System.Configuration;
using System.Web.Mvc;

namespace WebGallery.Filters
{
    public class RequireSubmittingAppEnabled : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if submitting app is enabled.
            if (ConfigurationManager.AppSettings["EnableSubmitApp"].ToLowerInvariant() == "false")
            {
                filterContext.Result = new ViewResult { ViewName = "SubmittingDisabled" };
            }

            base.OnActionExecuting(filterContext);
        }
    }
}