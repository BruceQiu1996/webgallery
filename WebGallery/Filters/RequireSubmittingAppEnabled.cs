using System;
using System.Configuration;
using System.Web.Mvc;

namespace WebGallery.Filters
{
    public class RequireSubmittingAppEnabled : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if submitting app is enabled.
            if (ConfigurationManager.AppSettings["EnableSubmitApp"].Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new ViewResult { ViewName = "SubmittingDisabled" };
            }

            base.OnActionExecuting(filterContext);
        }
    }
}