using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ForwarderController : Controller
    {
        // GET: Forwarder
        
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = db.FORWARDERS.Where(f => f.IsDeleted == false).ToList();
                return View(list);
            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( FORWARDER forwarder)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        // Set IsDeleted to false and save the forwarder to get the ID
                        forwarder.IsDeleted = false;
                        db.FORWARDERS.Add(forwarder);
                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
                
            }
            return View(forwarder);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var forwarder = db.FORWARDERS.Find(id);
                if (forwarder == null)
                    return HttpNotFound();

                return View(forwarder);
            }
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var forwarder = db.FORWARDERS.Find(id);
                if (forwarder == null)
                {
                    return HttpNotFound();
                }
                return View(forwarder);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FORWARDER forwarder)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (ModelState.IsValid)
                {
                    var forwarderToUpdate = db.FORWARDERS.Find(forwarder.ID);
                    if (forwarderToUpdate != null)
                    {
                        forwarderToUpdate.ForwarderName = forwarder.ForwarderName;
                        forwarderToUpdate.Director = forwarder.Director;
                        forwarderToUpdate.Description = forwarder.Description;
                        forwarderToUpdate.ContactPerson = forwarder.ContactPerson;
                        
                        db.Entry(forwarderToUpdate).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }

                    return View(forwarderToUpdate);
                }
                return View(forwarder);
            }
        }
        public ActionResult Delete(int? Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var forwarder = db.FORWARDERS.Find(Id);
                if (forwarder == null)
                {
                    return HttpNotFound();
                }

                return View(forwarder);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    FORWARDER forwarderToDelete = db.FORWARDERS.Find(ID);
                    if (forwarderToDelete != null)
                    {

                        forwarderToDelete.IsDeleted = true;
                        
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
        }

    }
}