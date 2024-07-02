using System.Data.Entity.Infrastructure;
using System.Net;
using System;
using System.Web.Mvc;
using tahsinERP.Models;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace tahsinERP.Controllers
{
    public class FTransportTypeController : Controller
    {
        // GET: FTransportType
        public ActionResult Index()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = db.F_TRANSPORT_TYPES.ToList();
                return View(list);
            }
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(F_TRANSPORT_TYPES transportType)
        {
            if (transportType == null)
            {
                ModelState.AddModelError("", "Invalid form data. Please check the inputs.");
                return View(transportType);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var transportTypeInsert = new F_TRANSPORT_TYPES();
                        transportTypeInsert = transportType;
                        db.F_TRANSPORT_TYPES.Add(transportTypeInsert);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            return View(transportType);
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var transportType = db.F_TRANSPORT_TYPES.Find(id);
                if (transportType == null)
                    return HttpNotFound();

                return View(transportType);
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

                var transportType = db.F_TRANSPORT_TYPES.Find(id);
                if (transportType == null)
                {
                    return HttpNotFound();
                }
                return View(transportType);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(F_TRANSPORT_TYPES transportType)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (ModelState.IsValid)
                {
                    var transportTypeToUpdate = db.F_TRANSPORT_TYPES.Find(transportType.ID);
                    if (transportTypeToUpdate != null)
                    {
                        if (TryUpdateModel(transportTypeToUpdate, "", new string[] { "TransportType, ExtLgth, ExtWdth, ExtHght, IntLgth, IntWdth, IntHght, Unit, CapableOfLifting, TransportWeight" }))
                        {
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

                    return View(transportTypeToUpdate);
                }
                return View(transportType);
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
                var transportType = db.F_TRANSPORT_TYPES.Find(Id);
                if (transportType == null)
                {
                    return HttpNotFound();
                }

                return View(transportType);
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
                    var  forwarderToDelete = db.F_TRANSPORT_TYPES.Find(ID);
                    if (forwarderToDelete != null)
                    {
                        db.F_TRANSPORT_TYPES.Remove(forwarderToDelete);

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