using System.ComponentModel.DataAnnotations;
using WebGallery.Models;

namespace WebGallery.ViewModels
{
    public class IssueReportViewModel
    {
        [Range(1, 2, ErrorMessage = "*")]
        [Display(Name = "Issue Type")]
        public IssueType IssueType { get; set; } = IssueType.PortalIssue;
        
        [Display(Name = "App Id")]
        [MaxLength(255)]
        public string AppId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*")]
        [MaxLength(4000)]
        [Display(Name = "Issue Description")]
        public string IssueDescription { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*")]
        [MaxLength(255)]
        [Display(Name = "Your First Name")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*")]
        [MaxLength(255)]
        [Display(Name = "Your Last Name")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*")]
        [EmailAddress]
        [MaxLength(500)]
        [Display(Name = "Your Email")]
        public string YourEmail { get; set; }

        [Compare("YourEmail")]
        [Display(Name = "Confirm Your Email")]
        public string ConfirmYourEmail { get; set; }
    }
}