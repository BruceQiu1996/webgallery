using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebGallery.Models;

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
        


    }
        
}