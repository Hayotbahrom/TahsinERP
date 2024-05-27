using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class PModuleController : Controller
    {
        DBTHSNEntities db = new DBTHSNEntities();
        // GET: PModule
        [HttpGet]
        public ActionResult Index()
        {
            var permissionModules = db.PERMISSIONMODULES.ToList();
            var permissions = db.PERMISSIONS.ToList();
            var roles = db.ROLES.ToList();

            var viewModel = permissionModules.Select(pm => new PermissionModuleViewModel
            {
                ID = pm.ID,
                Module = pm.Module,
                RoleNames = string.Join(", ", permissions
                                        .Where(p => p.PermissionModuleID == pm.ID)
                                        .Select(p => roles
                                                     .FirstOrDefault(r => r.ID == p.RoleID)?.RName)
                                        .Distinct())
            }).ToList();

            return View(viewModel);
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PERMISSIONMODULE permissionModule)
        {
            try
            {
                PERMISSIONMODULE permissions = new PERMISSIONMODULE();
                permissions.Module = permissionModule.Module;
                db.PERMISSIONMODULES.Add(permissions);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            return View();
        }

        public ActionResult Edit(PERMISSIONMODULE permissionModule)
        {
            var pmodule = db.PERMISSIONMODULES.FirstOrDefault(x => x.ID == permissionModule.ID);
            if (pmodule == null)
            {
                return HttpNotFound();
            }
            return View(pmodule);
        }

        [HttpPost,ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int Id)
        {
            var pModule = db.PERMISSIONMODULES.Find(Id);

            if (TryUpdateModel(pModule, "", new string[] {  "Module" }))
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int Id)
        {
            var permissionModule = db.PERMISSIONMODULES.Find(Id);

            if (permissionModule != null)
            {
                var permissions = db.PERMISSIONS
                    .Where(p => p.PermissionModuleID ==Id)
                    .ToList();

                var roleIds = permissions.Select(p => p.RoleID).Distinct().ToList();
                var roles = db.ROLES
                    .Where(r => roleIds.Contains(r.ID))
                    .ToList();

                foreach (var permission in permissions)
                {
                    db.PERMISSIONS.Remove(permission);
                }

                foreach (var role in roles)
                {
                    if (!db.PERMISSIONS.Any(p => p.RoleID == role.ID))
                    {
                        db.ROLES.Remove(role);
                    }
                }

                db.PERMISSIONMODULES.Remove(permissionModule);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}