using System.Web.Mvc;
using WebGallery.Security;
using WebGallery.Services;

namespace WebGallery.Filters
{
    public class RequireSubmittershipAttribute : ActionFilterAttribute
    {
        private ISubmitterService _submitterService;

        public RequireSubmittershipAttribute() : this(new SubmitterService()) { }

        public RequireSubmittershipAttribute(ISubmitterService submitterService)
        {
            _submitterService = submitterService;
        }

        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.User;

            // If the user is currently not a submtter, then go to account/profile.
            if (!user.IsSubmitter())
            {
                filterContext.Result = new RedirectResult($"/account/profile?returnUrl={filterContext.HttpContext.Request.RawUrl}");
                return;
            }

            // If current user is not Super Submitter, and there haven't recorded his/her contact info in this system,
            // then go to account/profile.
            var hasContactInfo = await _submitterService.HasContactInfoAsync(user.GetSubmittership().SubmitterID);
            if (!user.IsSuperSubmitter() && !hasContactInfo)
            {
                filterContext.Result = new RedirectResult($"/account/profile?returnUrl={filterContext.HttpContext.Request.RawUrl}");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}