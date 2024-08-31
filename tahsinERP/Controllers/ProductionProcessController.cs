using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ProductionProcessController : Controller
    {
        // GET: ProductionProcess
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var process = db.PRODUCTIONPROCESSES.Where(pr => pr.IsDeleted == false).ToList();
                return View(process);
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(PRODUCTIONPROCESS productionProcess)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        productionProcess.IsDeleted = false;
                        db.PRODUCTIONPROCESSES.Add(productionProcess);
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "ProductionProccessController", $"{productionProcess.ProcessName} - ProductProccessni yaratdi");

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                }
                return View(productionProcess);
            }
        }

        public ActionResult Delete(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var process = db.PRODUCTIONPROCESSES.Find(id);
                if (process == null)
                {
                    return HttpNotFound();
                }
                return View(process);
            }
        }
        [HttpPost]
        public ActionResult Delete(PRODUCTIONPROCESS pRODUCTIONPROCESS)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var productionProcess_deleted = db.PRODUCTIONPROCESSES.Find(pRODUCTIONPROCESS.ID);
                    if (productionProcess_deleted != null)
                    {
                        productionProcess_deleted.IsDeleted = true;
                        if (TryUpdateModel(productionProcess_deleted, "", new string[] { "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "ProductionProccessController", $"{productionProcess_deleted.ProcessName} - ProductProccessni o'chirdi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(productionProcess_deleted);
                }
            }

            return View();
        }

        public ActionResult Edit(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var process = db.PRODUCTIONPROCESSES.Find(id);
                if (process == null)
                {
                    return HttpNotFound();
                }
                return View(process);
            }
        }
        [HttpPost]
        public ActionResult Edit(PRODUCTIONPROCESS pRODUCTIONPROCESS)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var process_update = db.PRODUCTIONPROCESSES.Find(pRODUCTIONPROCESS.ID);
                    if (process_update != null)
                    {
                        process_update.IsDeleted = false;
                        if (TryUpdateModel(process_update, "", new string[] { "ProcessName", "Description", "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "ProductionProccessController", $"{process_update.ProcessName} - ProductProccessni tahrirladi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(process_update);
                }
            }
            return View();
        }
        public ActionResult Details(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var process = db.PRODUCTIONPROCESSES.Find(id);
                if (process == null)
                {
                    return HttpNotFound();
                }
                return View(process);
            }
        }
    }
}