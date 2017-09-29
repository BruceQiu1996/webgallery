using System.Web.Mvc;

namespace WebGallery.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Error()
        {
            return View();
        }

        public ActionResult Error404()
        {
            return View("ResourceNotFound");
        }

        public ActionResult Fire()
        {
            throw new System.Exception("Testing custom errors ... you can ignore this.");
        }
    }
}