using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class RoleController : Controller
    {
        DBTHSNEntities db = new DBTHSNEntities();
        // GET: Role
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
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(ROLES role)
        {

            var roles = db.ROLES.FirstOrDefault(x => x.ID == role.ID); 
            if (role == null)
            {
                return HttpNotFound();
            }

            return View(roles);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id)
        {
            var roleToUpdate = db.ROLES.Find(id);

            if (TryUpdateModel(roleToUpdate, "", new string[] { "RName", "Description" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {

                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            } 
            return View();
            
            
        }
        [HttpGet]
        public ActionResult Delete(ROLES roles)
        {
            var role = db.ROLES.FirstOrDefault(x => x.ID == roles.ID);
            if(role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID)
        {
            ROLES role = db.ROLES.Find(ID);
            role.IsDeleted = true;
            if (TryUpdateModel(role,"", new string[] { "IsDeleted" }))
            {
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View();
        }

        public ActionResult Details(int? ID)
        {
            if(ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = db.ROLES.Find(ID);
            ROLES roles = new ROLES();

            roles.ID = role.ID;
            roles.RName = role.RName;
            roles.Description = role.Description;
            return View(roles);
        }

        public ActionResult PermissionsOfRole()
        {
            var roles = db.PERMISSIONS.Where(r => r.IsDeleted == false).ToList();
            return View(roles);
        }
    }

}