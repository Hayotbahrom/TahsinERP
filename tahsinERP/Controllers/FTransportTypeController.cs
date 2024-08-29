using System.Data.Entity.Infrastructure;
using System.Net;
using System;
using System.Web.Mvc;
using tahsinERP.Models;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Charts;
using tahsinERP.ViewModels;

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
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TransportTypeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Map ViewModel to Entity Model
                var transportType = new F_TRANSPORT_TYPES
                {
                    TransportType = viewModel.TransportType,
                    ExtLgth = viewModel.ExtLgth,
                    ExtWdth = viewModel.ExtWdth,
                    ExtHght = viewModel.ExtHght,
                    IntLgth = viewModel.IntLgth,
                    IntWdth = viewModel.IntWdth,
                    IntHght = viewModel.IntHght,
                    UnitID = viewModel.UnitID,
                    CapableOfLifting = viewModel.CapableOfLifting,
                    TransportWeight = viewModel.TransportWeight
                };

                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    try
                    {
                        db.F_TRANSPORT_TYPES.Add(transportType);
                        db.SaveChanges();

                        var userEmail = User.Identity.Name;
                        LogHelper.LogToDatabase(userEmail, "FTransportTypeController", $"{viewModel.ID} ID ga ega TransPortTypeni yaratdi");

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error: " + ex.Message);
                        ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                    }
                }
            }

            // If ModelState is not valid, return the view with the ViewModel
            return View(viewModel);
        }


        /*[HttpPost]
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
                        db.F_TRANSPORT_TYPES.Add(transportType);
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
        }*/
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var transportType = db.F_TRANSPORT_TYPES.Find(id);
                if (transportType == null)
                    return HttpNotFound();

                db.Entry(transportType).Reference(x => x.UNIT).Load();
                return View(transportType);
            }
        }
        // GET: FTransportType/Edit/
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Retrieve existing entity
                var transportType = db.F_TRANSPORT_TYPES.Find(id);
                if (transportType == null)
                {
                    return HttpNotFound();
                }
                db.Entry(transportType).Reference(x => x.UNIT).Load();

                // Map entity to view model
                var viewModel = new TransportTypeViewModel
                {
                    ID = transportType.ID,
                    TransportType = transportType.TransportType,
                    ExtLgth = transportType.ExtLgth,
                    ExtWdth = transportType.ExtWdth,
                    ExtHght = transportType.ExtHght,
                    IntLgth = transportType.IntLgth,
                    IntWdth = transportType.IntWdth,
                    IntHght = transportType.IntHght,
                    UnitID = (int)transportType.UnitID,
                    UNIT = transportType.UNIT,
                    CapableOfLifting = transportType.CapableOfLifting,
                    TransportWeight = transportType.TransportWeight
                };
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                return View(viewModel);
            }
        }
        // POST: FTransportType/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TransportTypeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    // Retrieve existing entity
                    var transportTypeToUpdate = db.F_TRANSPORT_TYPES.Find(viewModel.ID);
                    if (transportTypeToUpdate == null)
                    {
                        return HttpNotFound();
                    }

                    // Update entity with data from view model
                    transportTypeToUpdate.TransportType = viewModel.TransportType;
                    transportTypeToUpdate.ExtLgth = viewModel.ExtLgth;
                    transportTypeToUpdate.ExtWdth = viewModel.ExtWdth;
                    transportTypeToUpdate.ExtHght = viewModel.ExtHght;
                    transportTypeToUpdate.IntLgth = viewModel.IntLgth;
                    transportTypeToUpdate.IntWdth = viewModel.IntWdth;
                    transportTypeToUpdate.IntHght = viewModel.IntHght;
                   // transportTypeToUpdate.UnitID = viewModel.UnitID;
                    transportTypeToUpdate.CapableOfLifting = viewModel.CapableOfLifting;
                    transportTypeToUpdate.TransportWeight = viewModel.TransportWeight;

                    try
                    {
                        db.SaveChanges();

                        var userEmail = User.Identity.Name;
                        LogHelper.LogToDatabase(userEmail, "FTransportTypeController", $"{viewModel.ID} ID ga ega TransportTypeni tahrirladi");

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error: " + ex.Message);
                    }
                }
            }

            // If ModelState is not valid, return the view with the ViewModel
            return View(viewModel);
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
                db.Entry(transportType).Reference(x => x.UNIT).Load();

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

                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "FTransportTypeController", $"{ID} ID ga ega TransportTypeni o'chirdi");

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