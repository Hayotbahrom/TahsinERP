using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class RoleController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
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
        public ActionResult Create(ROLE role)
        {
            try
            {
                ROLE roles = new ROLE();
                roles.RName = role.RName;
                roles.Description = role.Description;
                roles.IsDeleted = false;

                db.ROLES.Add(roles);

                var length = db.PERMISSIONMODULES.ToList().Count;
                int[] permissionModulesID = new int[length];
                int i = 0;
                foreach (var item in db.PERMISSIONMODULES.ToList())
                {
                    permissionModulesID[i] = item.ID;

                    PERMISSION newPermission = new PERMISSION();
                    newPermission.PermissionModuleID = permissionModulesID[i];
                    newPermission.ChangePermit = false;
                    newPermission.ViewPermit = true;
                    newPermission.RoleID = role.ID;
                    db.PERMISSIONS.Add(newPermission);
                    i++;
                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(ROLE role)
        {
            var roles = db.ROLES.Include(r => r.PERMISSIONS).FirstOrDefault(x => x.ID == role.ID);
            if (role == null)
            {
                return HttpNotFound();
            }
            //Developer rolini faqat developer o`zgartirishga tekshirish
            var currentUserRole = User.Identity.Name;
            if (currentUserRole != "developer" && role.RName == "developer")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Only developers can edit the developer role.");
            }

            return View(roles);
        }
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id)
        {
            if (ModelState.IsValid)
            {
                var roleToUpdate = db.ROLES.Find(id);

                //Developer rolini faqat developer o`zgartirishga tekshirish
                var currentUserRole = User.Identity.Name;
                if (currentUserRole != "developer" && roleToUpdate.RName == "developer")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Only developers can edit the developer role.");
                }

                if (TryUpdateModel(roleToUpdate, "", new string[] { "RName", "Description" }))
                {
                    try
                    {
                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                    catch (RetryLimitExceededException)
                    {

                        ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Delete(ROLE roles)
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
            ROLE role = db.ROLES.Find(ID);
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
                    ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
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
            ROLE roles = new ROLE();

            roles.ID = role.ID;
            roles.RName = role.RName;
            roles.Description = role.Description;
            roles.PERMISSIONS = role.PERMISSIONS;
            return View(roles);
        }
        public ActionResult Permissions(int id)
        {
            using (var db = new DBTHSNEntities())
            {
                var role = db.ROLES
                    .Include(r => r.PERMISSIONS.Select(p => p.PERMISSIONMODULE))
                    .FirstOrDefault(r => r.ID == id);

                if (role == null)
                {
                    return HttpNotFound();
                }

                var groupedPermissions = role.PERMISSIONS
                    .GroupBy(p => p.PERMISSIONMODULE)
                    .Select(g => g.First())
                    .ToList();

                var viewModel = new RolePermissionsViewModel
                {
                    Role = role,
                    Permissions = groupedPermissions
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Permissions(ROLE role)
        {
            using (var db = new DBTHSNEntities())
            {
                var existingRole = db.ROLES.Include(r => r.PERMISSIONS).FirstOrDefault(r => r.ID == role.ID);
                if (existingRole == null)
                {
                    return HttpNotFound();
                }

                foreach (var permission in role.PERMISSIONS)
                {
                    var existingPermission = existingRole.PERMISSIONS.FirstOrDefault(p => p.ID == permission.ID);
                    if (existingPermission != null)
                    {
                        existingPermission.ViewPermit = permission.ViewPermit;
                        existingPermission.ChangePermit = permission.ChangePermit;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Edit", new { id = role.ID });
            }
        }

    }
}
