using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PackingListController : Controller
    {
        // GET: PackingList
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                List<P_INVOICE_PACKINGLISTS> list;

                list = db.P_INVOICE_PACKINGLISTS.Where(pl => pl.IsDeleted == false).ToList();

                // Explicitly load related entities
                foreach (var packingList in list)
                {
                    db.Entry(packingList).Reference(p => p.P_INVOICES).Load();
                }

                return View(list);
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                ViewBag.Invoice = new SelectList(db1.P_INVOICES.Where(p => p.IsDeleted == false).ToList(), "ID", "InvoiceNo");
                ViewBag.FTransportType = new SelectList(db1.F_TRANSPORT_TYPES.ToList(), "ID", "TransportType");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(P_INVOICE_PACKINGLISTS packingList)
        {
            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        packingList.IsDeleted = false;
                        db1.P_INVOICE_PACKINGLISTS.Add(packingList);

                        db1.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex);
                }
                ViewBag.Invoice = new SelectList(db1.P_INVOICES.Where(p => p.IsDeleted == false).ToList(), "ID", "InvoiceNo", packingList.InvoiceID);
                ViewBag.FTransportType = new SelectList(db1.F_TRANSPORT_TYPES.ToList(), "ID", "PName", packingList.TransportTypeID);
            }

            return View(packingList);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            P_INVOICE_PACKINGLISTS packingList;
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                packingList = db.P_INVOICE_PACKINGLISTS.Find(id);

                if (packingList == null)
                    return HttpNotFound();

                // Manually load the related entities
                db.Entry(packingList).Reference(i => i.P_INVOICES).Load();
                //db.Entry(packingList).Reference(i => i.PART).Load();
            }

            return View(packingList);
        }
        public ActionResult Delete(int? Id)
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var packingList = db.P_INVOICE_PACKINGLISTS.Find(Id);
                if (packingList == null)
                {
                    return HttpNotFound();
                }
                // Manually load the related entities
                db.Entry(packingList).Reference(i => i.P_INVOICES).Load();
                //db.Entry(packingList).Reference(i => i.PART).Load();

                return View(packingList);
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
                    P_INVOICE_PACKINGLISTS packingListToDelete = db.P_INVOICE_PACKINGLISTS.Find(ID);
                    if (packingListToDelete != null)
                    {
                        packingListToDelete.IsDeleted = true;
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
                    else
                    {
                        ModelState.AddModelError("", "Bunday shartnoma topilmadi.");
                    }
                }
            }
            return View();
        }
        public ActionResult Edit(int? ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var packingList = db.P_INVOICE_PACKINGLISTS.Find(ID);
                if (packingList == null)
                {
                    return HttpNotFound();
                }
                //ViewBag.Invoice = new SelectList(db.P_INVOICES, "ID", "InvoiceNo", packingList.InvoiceID);

                ViewBag.Invoice = new SelectList(db.P_INVOICES.ToList(), "ID", "InvoiceNo");
                ViewBag.Part = new SelectList(db.PARTS.ToList(), "ID", "PName");
                return View(packingList);

            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(P_INVOICE_PACKINGLISTS packingList)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (ModelState.IsValid)
                {
                    P_INVOICE_PACKINGLISTS packingListToUpdate = db.P_INVOICE_PACKINGLISTS.Find(packingList.ID);
                    if (packingListToUpdate != null)
                    {
                        
                        if (TryUpdateModel(packingListToUpdate, "", new string[]
                        {
                            "InvoiceID", "PackingListNo", "PartID", "PrLength", "PrWidth", "PrHeight",
                            "PrCMB", "PrGrWeight", "PrPackMaterial", "ScLength", "ScWidth", "ScHeight",
                            "ScCMB", "ScGrWeight", "ScPackMaterial", "PltType", "IsDeleted"
                        })) { 
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
                    return View(packingListToUpdate);
                }
                return View();
            }
        }
    }
}