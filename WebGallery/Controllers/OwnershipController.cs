using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Filters;
using WebGallery.Security;
using WebGallery.Services;

namespace WebGallery.Controllers
{
    public class OwnershipController : Controller
    {
        private IOwnershipService _ownershipService;
        private ISubmitterService _submitterService;

        public OwnershipController() : this(new OwnershipService(), new SubmitterService()) { }
        public OwnershipController(IOwnershipService ownershipService,
            ISubmitterService submitterService)
        {
            _ownershipService = ownershipService;
            _submitterService = submitterService;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> Remove(int submissionId, int submitterId)
        {
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId))
            {
                return View("NeedPermission");
            }

            await _ownershipService.RemoveAsync(submitterId, submissionId);

            return RedirectToRoute(SiteRouteNames.App_Owners, new { submissionId = submissionId });
        }
    }
}