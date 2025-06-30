using DocumentFormat.OpenXml.EMMA;
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
        // GET: Role
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var roles = db.ROLES.Where(r => r.IsDeleted == false).ToList();
                return View(roles);
            }
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
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    ROLE newRole = new ROLE();
                    newRole.RName = role.RName;
                    newRole.Description = role.Description;
                    newRole.IsDeleted = false;

                    db.ROLES.Add(newRole);
                    LogHelper.LogToDatabase(User.Identity.Name, "RoleController", $"{newRole.RName} - Roleni yaratdi");

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

                        LogHelper.LogToDatabase(User.Identity.Name, "RoleController", $"{newPermission.ROLE.RName} uchun - {newPermission.PERMISSIONMODULE.Module} Permissionni yaratdi");

                        i++;
                    }

                    db.SaveChanges();
                }

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
            using (DBTHSNEntities db = new DBTHSNEntities())
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
        }
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
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

                            LogHelper.LogToDatabase(User.Identity.Name, "RoleController", $"{roleToUpdate.RName} - Roleni tahrirladi");

                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Delete(ROLE roles)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var role = db.ROLES.FirstOrDefault(x => x.ID == roles.ID);
                if (role == null)
                {
                    return HttpNotFound();
                }

                return View(role);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ROLE role = db.ROLES.Find(ID);
                role.IsDeleted = true;
                if (TryUpdateModel(role, "", new string[] { "IsDeleted" }))
                {
                    try
                    {
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "RoleController", $"{role.RName} - Roleni o'chirdi");

                        return RedirectToAction("Index");
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }
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

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var role = db.ROLES.Find(ID);
                ROLE roles = new ROLE();

                roles.ID = role.ID;
                roles.RName = role.RName;
                roles.Description = role.Description;
                roles.PERMISSIONS = role.PERMISSIONS;
                return View(roles);
            }
        }
        public ActionResult Permissions(int id)
        {
            using (var db = new DBTHSNEntities())
            {
                ROLE role = db.ROLES
                    .Include(r => r.PERMISSIONS.Select(p => p.PERMISSIONMODULE))
                    .FirstOrDefault(r => r.ID == id);

                if (role == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new RolePermissionsViewModel
                {
                    RoleID = role.ID,
                    RoleName = role.RName,
                    Description = role.Description,
                    Permissions = role.PERMISSIONS.Select(p => new PermissionViewModel
                    {
                        ID = p.ID,
                        PermissionModuleID = p.PermissionModuleID,
                        Module = p.PERMISSIONMODULE.Module,
                        Controller = p.PERMISSIONMODULE.Controller,
                        Action = p.PERMISSIONMODULE.Action,
                        ViewPermit = p.ViewPermit,
                        ChangePermit = p.ChangePermit
                    }).ToList()
                };

                return View(viewModel);
            }
        }
        [HttpPost]
        public ActionResult Permissions(RolePermissionsViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new DBTHSNEntities())
                {
                    var role = db.ROLES.Include(r => r.PERMISSIONS)
                                       .FirstOrDefault(r => r.ID == model.RoleID);

                    if (role != null)
                    {
                        foreach (var permViewModel in model.Permissions)
                        {
                            var permission = role.PERMISSIONS
                                                .FirstOrDefault(p => p.ID == permViewModel.ID);
                            if (permission != null)
                            {
                                permission.ViewPermit = permViewModel.ViewPermit;
                                permission.ChangePermit = permViewModel.ChangePermit;
                            }
                        }

                        db.SaveChanges();
                        LogHelper.LogToDatabase(User.Identity.Name, "RoleController", $"{role.RName} uchun Permissionni tahrirladi");
                        return RedirectToAction("Edit", new { id = role.ID });
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
