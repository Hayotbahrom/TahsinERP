using System;
using System.Data.Entity.Infrastructure;
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
                role.IsDeleted = false;

                db.ROLES.Add(roles);
                int[] permissionModulesID = new int[50];
                int i = 0;
                foreach (var item in db.PERMISSIONMODULES.ToList())
                {
                    permissionModulesID[i] = item.ID;

                    PERMISSIONS newPermission = new PERMISSIONS();
                    newPermission.PermissionModuleID = permissionModulesID[i];
                    newPermission.ChangePermit = false;
                    newPermission.ViewPermit = true;
                    newPermission.RoleID = role.ID;
                    db.PERMISSIONS.Add(newPermission);
                    i++;
                }
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
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID)
        {
            ROLES role = db.ROLES.Find(ID);
            role.IsDeleted = true;
            if (TryUpdateModel(role, "", new string[] { "IsDeleted" }))
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
            if (ID == null)
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
        public ActionResult Permissions(int? roleID)
        {
            var thisRole = db.ROLES.Find(roleID);

            return View(thisRole);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Permissions(int? roleID, FormCollection fvm)
        {
            var permissions = db.PERMISSIONS.Where(pr => pr.RoleID == roleID).ToList();
            foreach (var permission in permissions)
            {
                bool changePermit = permission.ChangePermit;
                bool viewPermit = permission.ViewPermit;

                // Update the database with the new permission values
                var perm = db.PERMISSIONS.Find(permission.ID);
                if (perm != null)
                {
                    perm.ChangePermit = changePermit;
                    perm.ViewPermit = viewPermit;
                    db.Entry(perm).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }

            // Redirect to a relevant page after saving the changes
            return RedirectToAction("Index");
        }

        public class PermissionViewModel
        {
            public int PermissionId { get; set; }
            public bool ChangePermit { get; set; }
            public bool ViewPermit { get; set; }
        }
    }

}