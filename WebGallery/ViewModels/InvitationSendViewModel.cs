using System.ComponentModel.DataAnnotations;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class InvitationSendViewModel
    {
        public Submission Submission { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "FirstNameRequired", ErrorMessageResourceType = typeof(Resources.InvitationSend))]
        [Display(Name = "FirstNameLabel", ResourceType = typeof(Resources.InvitationSend))]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "LastNameRequired", ErrorMessageResourceType = typeof(Resources.InvitationSend))]
        [Display(Name = "LastNameLabel", ResourceType = typeof(Resources.InvitationSend))]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "EMailRequired", ErrorMessageResourceType = typeof(Resources.InvitationSend))]
        [Display(Name = "eMailLabel", ResourceType = typeof(Resources.InvitationSend))]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "ConfirmationEMailRequired", ErrorMessageResourceType = typeof(Resources.InvitationSend))]
        [Compare("EmailAddress", ErrorMessageResourceName = "EMailAddressesDoNotMatch", ErrorMessageResourceType = typeof(Resources.InvitationSend))]
        [Display(Name = "ConfirmEmailLabel", ResourceType = typeof(Resources.InvitationSend))]
        public string ConfirmEmailAddress { get; set; }
    }
}