using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class InvitationDetailViewModel
    {
        public Submission Submission { get; set; }
        public UnconfirmedSubmissionOwner Invitation { get; set; }
    }
}