using System;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels.Home;

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
                ViewBag.ShowCookieConsent = true;
            }
            StatisticsVM model = new StatisticsVM();
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                model.NoOfProducts = db.PRODUCTS.Where(p => p.IsDeleted == false).ToList().Count;
                model.NoOfParts = db.PARTS.Where(p => p.IsDeleted == false).ToList().Count;
                model.NoOfUsers = db.USERS.Where(u => u.IsDeleted == false && u.IsActive == true).ToList().Count;
                model.NoOfRoles = db.ROLES.Where(r => r.IsDeleted == false).ToList().Count;
                model.NoOfParentBOMs = db.BOMS.Where(b => b.IsDeleted == false && b.IsParentProduct == true).Select(b => b.ParentPNo).Distinct().Count();
            }
            return View(model);
        }
        public JsonResult CookieConfirm()
        {
            try
            {
                var userEmail = User.Identity.Name;
                CookieHelper.Confirm(userEmail);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
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