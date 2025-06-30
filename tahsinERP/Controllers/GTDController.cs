using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class GTDController : Controller
    {
        // GET: GTD
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = db.GTDS.Where(x => x.IsDeleted == false).ToList();
                return View();
            }
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GTD model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var existModel = db.GTDS.Where(x => x.IsDeleted == false && x.GTD_No.ToLower().CompareTo(model.GTD_No.ToLower()) == 0).FirstOrDefault();
                        if (existModel is null)
                        {
                            model.IsDeleted = false;
                            db.GTDS.Add(model);
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "GTDController", $"{model.GTD_No} - GTD ni yaratdi");

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Bunday nom bilan Ta'minotchi kiritilgan, ma'lumotlarni qaytadan tekshiring.");
                            return View(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, ex);
                }
                return View(model);
            }
        }
        public ActionResult Details(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var gtd = db.GTDS.Find(Id);
                if (gtd == null)
                {
                    return HttpNotFound();
                }
                return View(gtd);
            }
        }
        public ActionResult Edit(int? Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var gtd = db.GTDS.Find(Id);
                if (gtd == null)
                {
                    return HttpNotFound();
                }
                return View(gtd);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SUPPLIER gtd)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    SUPPLIER supplierToUpdate = db.SUPPLIERS.Find(gtd.ID);
                    if (supplierToUpdate != null)
                    {
                        supplierToUpdate.IsDeleted = false;
                        supplierToUpdate.Type = gtd.Type;
                        if (TryUpdateModel(supplierToUpdate))
                        {
                            try
                            {
                                var existModel = db.SUPPLIERS.Where(x => x.IsDeleted == false && x.Name.ToLower().CompareTo(gtd.Name.ToLower()) == 0 && x.ID != supplierToUpdate.ID).FirstOrDefault();
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "SupplierController", $"{supplierToUpdate.Name} - Supplierni tahrirladi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(supplierToUpdate);
                }
            }
            return View();
        }
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var gtd = db.SUPPLIERS.Find(Id);
                if (gtd == null)
                {
                    return HttpNotFound();
                }

                return View(gtd);
            }
            //return RedirectToAction("SupplierParts?supplierId="+gtd.ID);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    SUPPLIER supplierToDelete = db.SUPPLIERS.Find(ID);
                    if (supplierToDelete != null)
                    {
                        supplierToDelete.IsDeleted = true;
                        if (TryUpdateModel(supplierToDelete, "", new string[] { "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "SupplierController", $"{supplierToDelete.Name} - Supplierni o'chirdi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                }
            }
            return View();
        }
    }
}