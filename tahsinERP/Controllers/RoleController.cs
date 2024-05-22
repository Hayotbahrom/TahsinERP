using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class RoleController : Controller
    {
        DBTHSNEntities db = new DBTHSNEntities();
        // GET: Role
        DBTHSNEntities db = new DBTHSNEntities();
        public ActionResult Index()
        {
            var roles = db.ROLES.Where(r => r.IsDeleted == false).ToList();
            return View(roles);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(ROLES role)
        {
            try
            {
                ROLES roles = new ROLES();
                roles.RName = role.RName;
                roles.Description = role.Description;
                roles.PERMISSIONS = role.PERMISSIONS;
                role.IsDeleted = false;

                db.ROLES.Add(roles);
                db.SaveChanges();

                return View(roles);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            return View();
        }

        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ROLES role = new ROLES();

            var editrole = db.ROLES.Find(ID);
            if (editrole == null)
            {
                return HttpNotFound();
            }
            else
            {
                role.RName = editrole.RName;
                role.Description = editrole.Description;
            }

            return View(role);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? ID, FormCollection collection)
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult Edit()
        {
            return View();
        }
        public ActionResult Delete()
        {
            return View();
        }
        public ActionResult Details(int ID)
        {
            return View();
        }
    }
}