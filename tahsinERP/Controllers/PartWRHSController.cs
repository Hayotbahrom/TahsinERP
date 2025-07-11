﻿using DocumentFormat.OpenXml.EMMA;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PartWRHSController : Controller
    {
        // GET: PartWRHS
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var part_wrhs = db.PART_WRHS
                          .Where(x => x.IsDeleted == false)
                          .Include(x => x.USER)
                          .ToList();
                return View(part_wrhs);
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // ViewBag orqali "Shop" ma'lumotlari
                var shops = db.SHOPS.ToList();
                ViewBag.ShopList = new SelectList(shops, "ID", "ShopName");

                // "MRP" ma'lumotlarini olish uchun
                var mrpUsers = db.USERS
                                    .Where(u => u.ROLES.Any(r => r.RName == "MRP"))
                                    .ToList();
                ViewBag.MRPUsers = new SelectList(mrpUsers.Where(x => x.IsDeleted == false ).ToList(), "ID", "FullName");

                return View();
            }
        }

        [HttpPost]
        public ActionResult Create(PART_WRHS model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    model.IsDeleted = false;
                    //account nimalini , nima uchun ishlatilishini tushunmadim, xatolik bermasligi uchun shunday yozildi.
                   // model.Account = model.MRP;
                    db.PART_WRHS.Add(model);
                    db.SaveChanges();
                    
                    LogHelper.LogToDatabase(User.Identity.Name, "PartWRHSController", $"{model.WHName} - PartWRHSni yaratdi");
                    
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ViewBag.ShopList = new SelectList(db.SHOPS.ToList(), "ID", "ShopName");
                    var mrpUsers = db.USERS
                                   .Where(u => u.ROLES.Any(r => r.RName == "MRP"))
                                   .ToList();
                    ViewBag.MRPUsers = new SelectList(mrpUsers.Where(x => x.IsDeleted == false).ToList(), "ID", "FullName");
                    return View(model);
                }
            }
        }
        private readonly DBTHSNEntities db = new DBTHSNEntities();

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                PART_WRHS partWRHS = db.PART_WRHS.Include(p => p.USER).FirstOrDefault(p => p.ID == id);
                if (partWRHS == null)
                {
                    return HttpNotFound();
                }

                ViewBag.MRPUsers = new SelectList(db.USERS.Where(u => u.ROLES.Any(r => r.RName == "MRP")), "ID", "FullName", partWRHS.MRP);
                ViewBag.ShopList = new SelectList(db.SHOPS, "ID", "ShopName", partWRHS.ShopID);
                return View(partWRHS);
            }
            catch (Exception ex)
            {
                // Log the error (uncomment the following line to write to a log file)
                // Log.Error(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: PartWRHS/Edit/5
        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,WHName,Description,IsDeleted,MRP,ShopID")] PART_WRHS partWRHS)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    partWRHS.IsDeleted = false;
                    db.Entry(partWRHS).State = EntityState.Modified;
                    db.SaveChanges();

                    LogHelper.LogToDatabase(User.Identity.Name, "PartWRHSController", $"{partWRHS.WHName} - PartWRHSni tahrirladi");

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewBag.MRPUsers = new SelectList(db.USERS.Where(u => u.ROLES.Any(r => r.RName == "MRP")), "ID", "FullName", partWRHS.MRP);
            ViewBag.ShopList = new SelectList(db.SHOPS, "ID", "ShopName", partWRHS.ShopID);
            return View(partWRHS);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PART_WRHS partWRHS = db.PART_WRHS.Find(id);
            if (partWRHS == null)
            {
                return HttpNotFound();
            }

            return View(partWRHS);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                PART_WRHS partWRHS = db.PART_WRHS.Find(id);
                if (partWRHS != null)
                {
                    partWRHS.IsDeleted = true;
                    db.Entry(partWRHS).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    LogHelper.LogToDatabase(User.Identity.Name, "PartWRHSController", $"{partWRHS.WHName} - PartWRHS o'chirdi");
                }


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                PART_WRHS partWRHS = db.PART_WRHS.Include(p => p.USER).Include(p => p.SHOP).FirstOrDefault(p => p.ID == id);
                if (partWRHS == null)
                {
                    return HttpNotFound();
                }

                return View(partWRHS);
            }
            catch (Exception ex)
            {
                // Log the error (uncomment the following line to write to a log file)
                // Log.Error(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}