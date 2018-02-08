using System.Web.Mvc;
using System.Web.Routing;
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

            var routeValues = new RouteValueDictionary();
            routeValues.Add("returnUrl", filterContext.HttpContext.Request.RawUrl);

            // If the user is currently not a submtter, then go to SiteRouteNames.Profile.
            if (!user.IsSubmitter())
            {
                filterContext.Result = new RedirectToRouteResult(SiteRouteNames.Profile, routeValues);
                return;
            }

            // If current user is not Super Submitter, and there haven't recorded his/her contact info in this system,
            // then go to SiteRouteNames.Profile.
            var hasContactInfo = await _submitterService.HasContactInfoAsync(user.GetSubmittership().SubmitterID);
            if (!user.IsSuperSubmitter() && !hasContactInfo)
            {
                filterContext.Result = new RedirectToRouteResult(SiteRouteNames.Profile, routeValues);
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}