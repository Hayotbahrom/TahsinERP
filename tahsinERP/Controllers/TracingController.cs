using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class TracingController : Controller
    {
        // GET: Tracing
        public async Task<ActionResult> Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = await db.TRACINGS.Where(p => p.IsDeleted == false).ToListAsync();
                return View(list);
            }
        }
        public async Task<ActionResult> Create()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync());
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TRACING tracing)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
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
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var tracing = await db.TRACINGS.FindAsync(id);
                if (tracing == null)
                    return HttpNotFound();

                return View(tracing);
            }
        }
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