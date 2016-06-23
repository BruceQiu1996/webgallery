using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Filters;
using WebGallery.Security;
using WebGallery.Services;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class InvitationController : Controller
    {
        private IAppService _appService;
        private IOwnershipService _ownershipService;
        private ISubmitterService _submitterService;
        private IEmailService _emailService;

        public InvitationController() : this(new AppService(), new OwnershipService(), new SubmitterService(), new EmailService()) { }
        public InvitationController(IAppService appService,
            IOwnershipService ownershipService,
            ISubmitterService submitterService,
            IEmailService emailService)
        {
            _appService = appService;
            _ownershipService = ownershipService;
            _submitterService = submitterService;
            _emailService = emailService;
        }

        [Authorize]
        [HttpGet]
        [RequireSubmittership]
        public async Task<ActionResult> Send(int? id)
        {
            if (!id.HasValue) return View("ResourceNotFound");

            var submissionId = id.Value;
            var submission = await _appService.GetSubmissionAsync(submissionId);
            if (submission == null)
            {
                return View("ResourceNotFound");
            }

            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submission.SubmissionID))
            {
                return View("NeedPermission");
            }

            return View(new InvitationSendViewModel { Submission = submission });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> Send(InvitationSendViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var submission = await _appService.GetSubmissionAsync(model.Submission.SubmissionID);
            if (submission == null)
            {
                return View("ResourceNotFound");
            }

            model.Submission = submission;
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, model.Submission.SubmissionID))
            {
                return View("NeedPermission");
            }

            // the one is alreay an owner of this submission
            if (await _ownershipService.HasOwnershipAsync(model.FirstName, model.LastName, model.Submission))
            {
                return View("AlreadyHasOwnership", model);
            }

            // the one has already been invited
            if (await _ownershipService.HasBeenInvitedAsync(model.FirstName, model.LastName, model.Submission))
            {
                return View("AlreadyBeenInvited", model);
            }

            var unconfirmedSubmissionOwner = await _ownershipService.CreateInvitationAsync(model.FirstName, model.LastName, model.Submission);

            // email the ownership invitation
            await _emailService.SendOwnershipInvitation(model.EmailAddress, unconfirmedSubmissionOwner, HttpContext.Request.Url.Authority, html => { return HttpContext.Server.HtmlEncode(html); });

            return RedirectToAction("Owners", "App", new { id = model.Submission.SubmissionID });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Detail(Guid? id)
        {
            if (!id.HasValue) return View("ResourceNotFound");

            var invitationGuid = id.Value;
            var invitation = await _ownershipService.GetInvitationAsync(invitationGuid);
            if (invitation == null)
            {
                return View("InvitationNotFound", invitationGuid);
            }

            var submission = await _appService.GetSubmissionAsync(invitation.SubmissionID);
            if (submission == null)
            {
                return View("InvitationAppNotFound", invitation.SubmissionID);
            }

            var model = new InvitationDetailViewModel
            {
                Invitation = invitation,
                Submission = submission
            };

            if (_ownershipService.IsInvitationExpired(invitation))
            {
                return View("InvitationExpired", model);
            }

            var submitter = User.GetSubmittership();
            if (submitter != null && await _submitterService.IsOwnerAsync(submitter.SubmitterID, submission.SubmissionID))
            {
                // if current user has the submittership, and is already the owner of the app within the invitation
                // redirect him/her to the portal
                return RedirectToAction("mine", "app");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> Accept(Guid invitationGuid)
        {
            var invitation = await _ownershipService.GetInvitationAsync(invitationGuid);
            if (invitation == null)
            {
                return View("InvitationNotFound", invitationGuid);
            }

            var submission = await _appService.GetSubmissionAsync(invitation.SubmissionID);
            if (submission == null)
            {
                return View("InvitationAppNotFound", invitation.SubmissionID);
            }

            var invitee = User.GetSubmittership();
            await _ownershipService.CreateAsync(invitee, submission, invitation);

            return RedirectToAction("Mine", "App");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Decline(Guid invitationGuid)
        {
            await _ownershipService.RemoveInvitationAsync(invitationGuid);

            return RedirectToAction("Mine", "App");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireSubmittership]
        public async Task<ActionResult> Revoke(Guid invitationGuid, int submissionId)
        {
            if (!User.IsSuperSubmitter() && !await _submitterService.IsOwnerAsync(User.GetSubmittership().SubmitterID, submissionId))
            {
                return View("NeedPermission");
            }

            await _ownershipService.RemoveInvitationAsync(invitationGuid);

            return RedirectToAction("Owners", "App", new { id = submissionId });
        }
    }
}