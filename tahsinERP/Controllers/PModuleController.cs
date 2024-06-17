using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class PModuleController : Controller
    {
        // GET: PModule
        [HttpGet]
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
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
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PERMISSIONMODULE permissionModule)
        {
            try
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PERMISSIONMODULE permissions = new PERMISSIONMODULE();
                    permissions.Module = permissionModule.Module;
                    db.PERMISSIONMODULES.Add(permissions);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
            }
            return View();
        }
        public ActionResult Edit(PERMISSIONMODULE permissionModule)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var pmodule = db.PERMISSIONMODULES.FirstOrDefault(x => x.ID == permissionModule.ID);
                if (pmodule == null)
                {
                    return HttpNotFound();
                }
                List<ROLE> selectedRoles = db.ROLES.Join(db.PERMISSIONS, role => role.ID, permission => permission.RoleID, (role, permission) => new { role, permission }).Join(db.PERMISSIONMODULES, combined => combined.permission.PermissionModuleID, pm => pm.ID, (combined, pm) => new { combined.role, pm }).Where(combined => combined.pm.ID == pmodule.ID).Select(combined => combined.role).ToList();
           
                ViewBag.RoleID = new MultiSelectList(db.ROLES, "ID", "RName", selectedRoles);

                return View(pmodule);
            }
        }
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var pModule = db.PERMISSIONMODULES.Find(Id);

                if (TryUpdateModel(pModule, "", new string[] { "Module" }))
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
                return View();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var permissionModule = db.PERMISSIONMODULES.Find(Id);

                if (permissionModule != null)
                {
                    var permissions = db.PERMISSIONS
                        .Where(p => p.PermissionModuleID == Id)
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
        public ActionResult Details(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var permissionModule = db.PERMISSIONMODULES
                                          .FirstOrDefault(pm => pm.ID == id);
                if (permissionModule == null)
                {
                    return HttpNotFound();
                }

                var permissions = db.PERMISSIONS
                                    .Where(p => p.PermissionModuleID == id)
                                    .ToList();
                var roles = db.ROLES.ToList();

                var viewModel = new PermissionModuleViewModel
                {
                    ID = permissionModule.ID,
                    Module = permissionModule.Module,
                    RoleNames = string.Join(", ", permissions
                                                .Select(p => roles
                                                             .FirstOrDefault(r => r.ID == p.RoleID)?.RName)
                                                .Distinct())
                };

                return View(viewModel);
            }
        }

    }
}