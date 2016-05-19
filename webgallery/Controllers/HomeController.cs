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

        public ActionResult Gallery()
        {
            return View();
        }

        public ActionResult Documentation()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            return View(new ContactModel());
        }

        [HttpPost]
        public ActionResult Contact(ContactModel contactVM)
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

        public async Task<ActionResult> Developer()
        {
            var model = new HomeDeveloperViewModel();

            return View(model);
        }

        public async Task<ActionResult> Install()
        {
            var model = new HomeInstallViewModel();

            return View(model);
        }
    }
}