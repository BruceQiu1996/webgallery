using System.ComponentModel.DataAnnotations;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class InvitationSendViewModel
    {
        public Submission Submission { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "FirstNameRequired", ErrorMessageResourceType = typeof(Resources.Invitation))]
        [Display(Name = "FirstNameLabel", ResourceType = typeof(Resources.Invitation))]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "LastNameRequired", ErrorMessageResourceType = typeof(Resources.Invitation))]
        [Display(Name = "LastNameLabel", ResourceType = typeof(Resources.Invitation))]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "EMailRequired", ErrorMessageResourceType = typeof(Resources.Invitation))]
        [Display(Name = "eMailLabel", ResourceType = typeof(Resources.Invitation))]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "ConfirmationEMailRequired", ErrorMessageResourceType = typeof(Resources.Invitation))]
        [Compare("EmailAddress", ErrorMessageResourceName = "EMailAddressesDoNotMatch", ErrorMessageResourceType = typeof(Resources.Invitation))]
        [Display(Name = "ConfirmEmailLabel", ResourceType = typeof(Resources.Invitation))]
        public string ConfirmEmailAddress { get; set; }
    }
}