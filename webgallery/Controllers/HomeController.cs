using System.Threading.Tasks;
using System.Web.Mvc;
using WebGallery.Models;
using WebGallery.ViewModels;

namespace WebGallery.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Documents()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ReportIssue()
        {
            return View("Contact", new ContactModel());
        }

        [HttpPost]
        public ActionResult ReportIssue(ContactModel contactVM)
        {
            if (!ModelState.IsValid)
            {
                return View(contactVM);
            }

            var contact = new ContactModel
            {
                FirstName = contactVM.FirstName,
                LastName = contactVM.LastName,
                Email = contactVM.Email,
                Comment = contactVM.Comment
            };

            // Send email 

            return RedirectToAction("ContactConfirm");
        }

        public ActionResult ContactConfirm()
        {
            return View();
        }

        public async Task<ActionResult> Agreement()
        {
            var model = new HomeAgreementViewModel();

            return View(model);
        }

        public ActionResult Error(string message)
        {
            return View(message);
        }
    }
}