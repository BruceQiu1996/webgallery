using System.Collections.Generic;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class AppOwnersViewModel
    {
        public Submission Submission { get; set; }
        public IList<SubmittersContactDetail> Owners { get; set; }
        public IList<UnconfirmedSubmissionOwner> OwnershipInvitations { get; set; }
    }
}