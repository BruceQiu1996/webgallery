using System.ComponentModel.DataAnnotations;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class InvitationSendViewModel
    {
        public Submission Submission { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Compare("EmailAddress")]
        [Display(Name = "Confirm Email Address")]
        public string ConfirmEmailAddress { get; set; }
    }
}