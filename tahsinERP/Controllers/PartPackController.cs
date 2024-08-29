using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class PartPackController : Controller
    {
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var partpacks = db.PARTPACKS.Include(x => x.PART).Where(x  => x.IsDeleted == false).ToList();
                return View(partpacks);
            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var parts = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.PartsList = new SelectList(parts, "ID", "PNo");
                return View();
            }
        }
        [HttpPost]
        public ActionResult Create(PARTPACK model, int partID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    model.PartID = partID;
                    model.RegDate = DateTime.Now;
                    model.IsDeleted = false;

                    db.PARTPACKS.Add(model);
                    db.SaveChanges();

                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "PartPackController", $"{partID} ID ga ega PartPackni yaratdi");

                    return RedirectToAction("Index");
                }

                var parts = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.PartsList = new SelectList(parts, "ID", "PNo", model.PartID);

                return View(model);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PARTPACK partpack = db.PARTPACKS.FirstOrDefault(p => p.ID == id);

                    if (partpack == null)
                    {
                        return HttpNotFound();
                    }

                    var parts = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                    ViewBag.PartList = new SelectList(parts, "ID", "PNo", partpack.PartID);

                    return View(partpack);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error occurred while fetching data: " + ex.Message;
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult Edit(PARTPACK model, int partId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        model.IsDeleted = false;
                        model.RegDate = DateTime.Now;
                        model.PartID = partId;

                        db.Entry(model).State = EntityState.Modified;
                        db.Entry(model).Property(p => p.PartID).IsModified = true;
                        db.SaveChanges();
                    }

                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "PartPackController", $"{partId} ID ga ega PartPackni tahrirladi");

                    return RedirectToAction("Index");
                }

                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var parts = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                    ViewBag.PartList = new SelectList(parts, "ID", "PNo", model.PartID);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error occurred while saving data: " + ex.Message;
                return View("Error");
            }
        }

        public ActionResult Delete(int? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {

                        var partpack = db.PARTPACKS.Include(pr => pr.PART).FirstOrDefault(p => p.ID == id);
                        if (partpack == null)
                        {
                            return HttpNotFound();
                        }
                        return View(partpack);
                    }
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Malumotni olishda hatolik yuz berdi!");

                }

            }
            return View();
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PARTPACK partpack = db.PARTPACKS.Find(id);
                    if (partpack != null)
                    {
                        partpack.IsDeleted = true;
                        db.Entry(partpack).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "PartPackController", $"{id} ID ga ega PartPackni o'chirdi");

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public ActionResult Details(int? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        var partpack = db.PARTPACKS.Include(pr => pr.PART).FirstOrDefault(p => p.ID == id);
                        if (partpack == null)
                        {
                            return HttpNotFound();
                        }
                        return View(partpack);
                    }
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Malumotni olishda hatolik yuz berdi!");

                }

            }
            return View();
        }
    }
}