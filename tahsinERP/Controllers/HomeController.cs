using System;
using System.Web.Mvc;

namespace tahsinERP.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var userEmail = User.Identity.Name;
            if (CookieHelper.IsConfirmed(userEmail) == false)
            {
                Session["HasSeenCookieConsent"] = true;
                ViewBag.ShowCookieConsent = true;
            }
            else
            {
                ViewBag.ShowCookieConsent = false;
            }
            return View();
        }


        public JsonResult CookieConfirm()
        {
            try
            {
                var userEmail = User.Identity.Name;
                CookieHelper.Confirm(userEmail);

                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Ilova tavsif sahifasi.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Sizning aloqa sahifangiz.";

            return View();
        }
    }
}