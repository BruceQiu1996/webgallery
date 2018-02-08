using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Models;
using WebGallery.Security;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class IssueController : Controller
    {
        private IIssueService _issueService;
        private IEmailService _emailService;

        public IssueController() : this(new IssueService(), new EmailService()) { }
        public IssueController(IIssueService issueService, IEmailService emailService)
        {
            _issueService = issueService;
            _emailService = emailService;
        }

        [Authorize]
        [HttpGet]
        public ActionResult Report(string appId)
        {
            var model = new IssueReportViewModel
            {
                AppId = appId,
                IssueType = string.IsNullOrWhiteSpace(appId) ? IssueType.PortalIssue : IssueType.AppIssue,
                YourEmail = User.GetEmailAddress()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Report(IssueReportViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var issue = await _issueService.SaveAsync(model.IssueType, model.AppId, model.IssueDescription, model.FirstName, model.LastName, model.YourEmail);

            await _emailService.SendMessageForIssueReported(issue, html => { return HttpContext.Server.HtmlEncode(html); });

            return RedirectToRoute(SiteRouteNames.Portal);
        }
    }
}