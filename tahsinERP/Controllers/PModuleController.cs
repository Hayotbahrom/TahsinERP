using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
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
            ViewBag.Parametr = ConfigurationManager.AppSettings["Parametr"]?.Split(',').ToList() ?? new List<string>();
            ViewBag.ControllerNames = GetAllControllerNames();
            return View();
        }

        // POST: PModule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PERMISSIONMODULE permissionModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        PERMISSIONMODULE permissions = new PERMISSIONMODULE();
                        permissions.Module = permissionModule.Module;
                        permissions.Controller = permissionModule.Controller;
                        permissions.Action = permissionModule.Action;
                        permissions.Parameter = permissionModule.Parameter;
                        db.PERMISSIONMODULES.Add(permissions);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
            }
            ViewBag.Parametr = ConfigurationManager.AppSettings["Parametr"]?.Split(',').ToList() ?? new List<string>();
            ViewBag.ControllerNames = GetAllControllerNames();
            return View(permissionModule);
        }

        // Helper method to get all controller names
        // Helper method to get all controller names
        


        // AJAX endpoint to get actions for a controller
        // PModuleController.cs
        [HttpPost]
        public JsonResult GetActions(string controller)
        {
            try
            {
                var controllerType = Assembly.GetExecutingAssembly().GetTypes()
                    .FirstOrDefault(type => type.Name == $"{controller}Controller");

                if (controllerType != null)
                {
                    var actions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                        .Where(m => !m.IsDefined(typeof(NonActionAttribute)))
                        .Select(m => m.Name)
                        .ToList();

                    return Json(actions);
                }

                return Json(new List<string>());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Log or handle exception as needed
                return Json(new List<string>());
            }
        }
        [HttpPost]
        public ActionResult Edit(PERMISSIONMODULE permissions, int[] RoleID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var receivedID = permissions.ID;
                        System.Diagnostics.Debug.WriteLine($"Received PermissionModule ID: {receivedID}");

                        var pModule = db.PERMISSIONMODULES.Find(receivedID);
                        if (pModule == null)
                        {
                            // Log the issue
                            System.Diagnostics.Debug.WriteLine($"PermissionModule not found for ID: {receivedID}");
                            return HttpNotFound();
                        }

                        // Update the permission module
                        pModule.Module = permissions.Module;
                        pModule.Controller = permissions.Controller;
                        pModule.Action = permissions.Action;
                        pModule.Parameter = permissions.Parameter; // Ensure Parametr is updated here

                        // Get existing permissions for the permission module
                        var existingPermissions = db.PERMISSIONS
                            .Where(p => p.PermissionModuleID == pModule.ID)
                            .ToList();

                        // Remove permissions that are no longer selected
                        foreach (var permission in existingPermissions)
                        {
                            if (!RoleID.Contains(permission.RoleID ?? 0))
                            {
                                db.PERMISSIONS.Remove(permission);
                            }
                        }

                        // Add new permissions for newly selected roles
                        foreach (var roleId in RoleID)
                        {
                            if (!existingPermissions.Any(p => p.RoleID == roleId))
                            {
                                db.PERMISSIONS.Add(new PERMISSION
                                {
                                    PermissionModuleID = pModule.ID,
                                    RoleID = roleId,
                                    ViewPermit = true,
                                    ChangePermit = false
                                });
                            }
                        }

                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        ModelState.AddModelError("", "Malumotlar boshqa foydalanuvchi tomonidan o'zgartirilgan yoki o'chirilgan. Qayta urinib ko'ring.");
                    }
                    catch (RetryLimitExceededException)
                    {
                        ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }
                }

                // Repopulate the ViewBag with roles and selected RoleID if model state is invalid
                var roles = db.ROLES.ToList();
                ViewBag.Roles = new MultiSelectList(roles, "ID", "RName", RoleID);
                ViewBag.ControllerNames = new SelectList(GetAllControllerNames(), "Value", "Text", permissions.Controller);
                ViewBag.Parametr = ConfigurationManager.AppSettings["Parametr"]?.Split(',').ToList() ?? new List<string>();

                return View(permissions);
            }
        }


        public ActionResult Edit(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var pModule = db.PERMISSIONMODULES.Find(id);
                if (pModule == null)
                {
                    return HttpNotFound();
                }

                var roles = db.ROLES.ToList();
                var selectedRoleIDs = db.PERMISSIONS.Where(p => p.PermissionModuleID == id).Select(p => p.RoleID).ToList();

                ViewBag.Roles = new MultiSelectList(roles, "ID", "RName", selectedRoleIDs);
                ViewBag.ControllerNames = new SelectList(GetAllControllerNames(), "Value", "Text", pModule.Controller);
                ViewBag.Parametr = ConfigurationManager.AppSettings["Parametr"]?.Split(',').ToList() ?? new List<string>();
                return View(pModule);
            }
        }


        private List<SelectListItem> GetAllControllerNames()
        {
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(type => type.Name.Replace("Controller", ""))
                .ToList();

            return controllers.Select(c => new SelectListItem { Text = c, Value = c }).ToList();
        }



        private DBTHSNEntities db = new DBTHSNEntities();

        // Other actions...

        public ActionResult Delete(int id)
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
                Controller = permissionModule.Controller,
                Action = permissionModule.Action,
                Parametr = permissionModule.Parameter,
                RoleNames = string.Join(", ", permissions
                                                .Select(p => roles
                                                             .FirstOrDefault(r => r.ID == p.RoleID)?.RName)
                                                .Distinct())
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int Id)
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
                    Controller = permissionModule.Controller,
                    Action = permissionModule.Action,
                    Parametr = permissionModule.Parameter,
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