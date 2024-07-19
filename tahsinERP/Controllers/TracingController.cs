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



        public async Task<ActionResult> Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS
                    .Where(p => p.IsDeleted == false)
                    .ToListAsync(), "ID", "TransportNo");
                return View();
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
                    ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync(), "ID", "PackingListNo");
                    return View(tracing);
                }
                try
                {
                    if (ModelState.IsValid)
                    {
                        // Set IsDeleted to false and save the tracing to get the ID
                        tracing.IsDeleted = false;
                        db.TRACINGS.Add(tracing);
                        await db.SaveChangesAsync();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }

            }
            return View(tracing);
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
                                                    .Where(p=> p.IsDeleted == false && p.PackingListID == id)
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
                /*bool checkExistTracing = checkForTodaysInput(tracing.PackingListID, tracing.IssueDateTime); //await db.TRACINGS.Where(x => x.PackingListID == tracing.PackingListID && x.IssueDateTime.ToShortDateString().CompareTo(tracing.IssueDateTime.ToShortDateString()) == 0).FirstOrDefaultAsync();
                if (checkExistTracing)
                {
                    ModelState.AddModelError("", "Bu sana bilan ma'lumot allaqachon kiritilgan. Qaytadan urunib ko'ring!");
                    ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync(), "ID", "PackingListNo");

                    return View(tracing);
                }*/

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
                        return RedirectToAction("Index");
                    }

                    return View(tracingToUpdate);
                }
                ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync(), "ID", "PackingListNo");

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