using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class TracingController : Controller
    {
        // GET: Tracing
        public async Task<ActionResult> Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = await db.TRACINGS
                    .Include(p => p.P_INVOICE_PACKINGLISTS)
                    .Where(p => p.IsDeleted == false)
                    .GroupBy(t => t.P_INVOICE_PACKINGLISTS.TransportNo)
                    .Select(g => new TracingViewModel
                    {
                        TransportNo = g.Key,
                        LastIssueDateTime = g.Max(t => t.IssueDateTime),
                        LastTracing = g.OrderByDescending(t => t.IssueDateTime).FirstOrDefault(),
                        Tracings = g.OrderBy(t => t.IssueDateTime).ToList()
                    })
                    .ToListAsync();

                // Ensure the list is not null
                if (list == null)
                {
                    list = new List<TracingViewModel>();
                }

                return View(list);
            }
        }


        public async Task<JsonResult> GetInvoiceNo_Supplier(int packingListID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // PackingListni olish
                var packingList = await db.P_INVOICE_PACKINGLISTS
                                          .Where(x => x.IsDeleted == false && x.ID == packingListID)
                                          .Select(x => new { x.InvoiceID })
                                          .FirstOrDefaultAsync();

                // Agar packingList topilmasa, null qaytarish
                if (packingList == null)
                {
                    return Json(new { success = false, message = "Packing list not found." }, JsonRequestBehavior.AllowGet);
                }

                // Invoice'ni olish
                var invoice = await db.P_INVOICES
                                      .Where(x => x.IsDeleted == false && x.ID == packingList.InvoiceID)
                                      .Select(x => new { x.InvoiceNo, x.SupplierID })
                                      .FirstOrDefaultAsync();

                // Agar invoice topilmasa, null qaytarish
                if (invoice == null)
                {
                    return Json(new { success = false, message = "Invoice not found." }, JsonRequestBehavior.AllowGet);
                }

                // Supplier'ni olish
                var supplier = await db.SUPPLIERS
                                       .Where(x => x.IsDeleted == false && x.ID == invoice.SupplierID)
                                       .Select(x => new { x.Name })
                                       .FirstOrDefaultAsync();

                // Agar supplier topilmasa, null qaytarish
                if (supplier == null)
                {
                    return Json(new { success = false, message = "Supplier not found." }, JsonRequestBehavior.AllowGet);
                }

                // Natijani JSON ko'rinishida qaytarish
                return Json(new
                {
                    success = true,
                    invoiceNo = invoice.InvoiceNo,
                    supplierName = supplier.Name
                }, JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<ActionResult> Create()
        {
            LoadViewBags();

            TRACING model = new TRACING();
            model.IssueDateTime = DateTime.Now;

            return View(model);
        }
        private void LoadViewBags()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Get the list of packing lists that are not deleted
                var packingLists = db.P_INVOICE_PACKINGLISTS
                    .Where(p => p.IsDeleted == false)
                    .ToList();

                // Group by TransportNo and select the first item from each group to ensure distinct TransportNo values
                var distinctPackingLists = packingLists
                    .GroupBy(p => p.TransportNo)
                    .Select(g => g.First())
                    .ToList();

                // Create the SelectList with distinct TransportNo
                ViewBag.packingList = new SelectList(distinctPackingLists, "ID", "TransportNo");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TRACING tracing)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                bool checkExistTracing = checkForTodaysInput(tracing.PackingListID, tracing.IssueDateTime); //await db.TRACINGS.Where(x => x.PackingListID == tracing.PackingListID && x.IssueDateTime.ToShortDateString().CompareTo(tracing.IssueDateTime.ToShortDateString()) == 0).FirstOrDefaultAsync();
                if (checkExistTracing)
                {
                    ModelState.AddModelError("", "Bu sana bilan ma'lumot allaqachon kiritilgan. Qaytadan urunib ko'ring!");
                }
                try
                {
                    if (ModelState.IsValid)
                    {
                        // Set IsDeleted to false and save the tracing to get the ID
                        tracing.IsDeleted = false;
                        db.TRACINGS.Add(tracing);
                        setPackingListInTransit(tracing.PackingListID);
                        await db.SaveChangesAsync();

                        var userEmail = User.Identity.Name;
                        LogHelper.LogToDatabase(userEmail, "TracingController", "Create[Post]");
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }

            }
            LoadViewBags();
            return View(tracing);
        }
        private void setPackingListInTransit(int packingListID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                P_INVOICE_PACKINGLISTS packingList = db.P_INVOICE_PACKINGLISTS.Where(x => x.ID == packingListID).FirstOrDefault();
                if (packingList != null)
                {
                    packingList.InTransit = true;
                    db.Entry(packingList).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        private bool checkForTodaysInput(int packingListID, DateTime issueDateTime)
        {
            DateTime day = issueDateTime.Date;
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                TRACING tracing = db.TRACINGS.Where(tr => tr.PackingListID == packingListID && tr.IssueDateTime == day).FirstOrDefault();
                if (tracing != null)
                    return true;
                else
                    return false;
            }
        }
        public async Task<ActionResult> Details(int? id)
        {
            if (id is null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var existTracingTransportNo = await db.TRACINGS
                                                    .Include(p => p.P_INVOICE_PACKINGLISTS)
                                                    .Where(p => p.IsDeleted == false && p.PackingListID == id)
                                                    .FirstOrDefaultAsync();

                var tracingList = await db.TRACINGS
                    .Include(p => p.P_INVOICE_PACKINGLISTS)
                    .Where(p => p.IsDeleted == false && p.P_INVOICE_PACKINGLISTS.TransportNo.CompareTo(existTracingTransportNo.P_INVOICE_PACKINGLISTS.TransportNo) == 0)
                    .ToListAsync();

                if (tracingList == null || tracingList.Count == 0)
                    return HttpNotFound();

                return View(tracingList);
            }
        }
        /*public async Task<ActionResult> Details(string transportNo)
        {
            if (string.IsNullOrEmpty(transportNo))
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var tracingList = await db.TRACINGS
                    .Include(p => p.P_INVOICE_PACKINGLISTS)
                    .Where(p => p.IsDeleted == false && p.P_INVOICE_PACKINGLISTS.TransportNo == transportNo)
                    .ToListAsync();

                if (tracingList == null || tracingList.Count == 0)
                    return HttpNotFound();

                return View(tracingList);   
            }
        }*/
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var tracing = await db.TRACINGS.FindAsync(id);
                if (tracing == null)
                {
                    return HttpNotFound();
                }
                ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync(), "ID", "PackingListNo");

                return View(tracing);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TRACING tracing)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                
                if (ModelState.IsValid)
                {
                    var tracingToUpdate = await db.TRACINGS.FindAsync(tracing.ID);
                    if (tracingToUpdate != null)
                    {
                        tracingToUpdate.PackingListID = tracing.PackingListID;
                        tracingToUpdate.ActualLocation = tracing.ActualLocation;
                        tracingToUpdate.ActualDistanceToDestination = tracing.ActualDistanceToDestination;
                        tracingToUpdate.IssueDateTime = tracing.IssueDateTime;
                        tracingToUpdate.ETA = tracing.ETA;

                        db.Entry(tracingToUpdate).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();
                        var userEmail = User.Identity.Name;
                        LogHelper.LogToDatabase(userEmail, "TracingController", "Edit[Post]");
                        return RedirectToAction("Index");
                    }

                    return View(tracingToUpdate);
                }
                return View(tracing);
            }
        }
        public async Task<ActionResult> Delete(int? Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var tracing = await db.TRACINGS.FindAsync(Id);
                if (tracing == null)
                {
                    return HttpNotFound();
                }
                ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync(), "ID", "PackingListNo");

                return View(tracing);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? ID, FormCollection gfs)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    TRACING tracingToDelete = await db.TRACINGS.FindAsync(ID);
                    if (tracingToDelete != null)
                    {

                        tracingToDelete.IsDeleted = true;

                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "TracingController", "Delete[Post]");
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