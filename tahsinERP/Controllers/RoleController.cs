using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
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
                int[] permissionModulesID = new int[50];
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
            return View(roles);
        }
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id)
        {
            if (ModelState.IsValid)
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
        public ActionResult Permissions(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var role = db.ROLES.Include(r => r.PERMISSIONS).FirstOrDefault(r => r.ID == id);
            if (role == null)
            {
                return HttpNotFound();
            }

            return View(role);
        }

        [HttpPost]
        public ActionResult Permissions(ROLE role)
        {
            if (!ModelState.IsValid)
            {
                var permission = db.ROLES.Include(p => p.PERMISSIONS).FirstOrDefault(p => p.ID == role.ID);
                foreach (var permissionToEdit in permission.PERMISSIONS )
                {
                    var item = db.PERMISSIONS.Find(permissionToEdit.ID);
                    if (item != null)
                    {
                        item.ChangePermit = permissionToEdit.ChangePermit;
                        item.ViewPermit = permissionToEdit.ViewPermit;

                        db.Entry(item).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                return RedirectToAction($"Edit/{role.ID}");
            }
            return View("index");
        }
    }
}
