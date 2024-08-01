using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class DefectTypeController : Controller
    {
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var defecttype = db.DEFECT_TYPES.Where(x => x.IsDeleted == false).ToList();
                return View(defecttype);
            }
        }

        public ActionResult Details(int? ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var defect_type = db.DEFECT_TYPES.Find(ID);
                if (defect_type == null)
                {
                    return HttpNotFound();
                }
                return View(defect_type);
            }
        }
        public ActionResult Edit(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var defect_type = db.DEFECT_TYPES.Find(id);
                if (defect_type == null)
                {
                    return HttpNotFound();
                }
                return View(defect_type);
            }
        }
        [HttpPost]
        public ActionResult Edit(DEFECT_TYPES model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var defect_type = db.DEFECT_TYPES.Find(model.ID);
                    if (defect_type != null)
                    {
                        defect_type.IsDeleted = false;
                        if (TryUpdateModel(defect_type, "", new string[] { "DefectType", "Description", "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();
                                var userEmail = User.Identity.Name;
                                LogHelper.LogToDatabase(userEmail, "DefectTypeController", "Edit[Post]");
                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(defect_type);
                }
            }
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(DEFECT_TYPES model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        model.IsDeleted = false;
                        db.DEFECT_TYPES.Add(model);
                        db.SaveChanges();
                        var userEmail = User.Identity.Name;
                        LogHelper.LogToDatabase(userEmail, "DefectTypeController", "Create[Post]");
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                }
                return View(model);
            }
        }
        public ActionResult Delete(int? ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var defecttype = db.DEFECT_TYPES.Find(ID);
                if (defecttype == null)
                {
                    return HttpNotFound();
                }
                return View(defecttype);
            }
        }
        [HttpPost]
        public ActionResult Delete(DEFECT_TYPES model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var Defect_type_delete = db.SHOPS.Find(model.ID);
                    if (Defect_type_delete != null)
                    {
                        Defect_type_delete.IsDeleted = true;
                        if (TryUpdateModel(Defect_type_delete, "", new string[] { "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();
                                var userEmail = User.Identity.Name;
                                LogHelper.LogToDatabase(userEmail, "DefectTypeController", "Delete[Post]");
                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(Defect_type_delete);
                }
            }

            return View();
        }


    }
}