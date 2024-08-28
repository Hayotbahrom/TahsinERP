using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

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
        /*
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

            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "PackingListController", "Create[Post]");
            return View(packingList);
        }
        */
        public ActionResult Create(int invoiceId)
        {
            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                // Populate the Invoice dropdown
                ViewBag.Invoice = db1.P_INVOICES.Where(p => p.IsDeleted == false && p.ID == invoiceId).Select(p => p.InvoiceNo).ToString();

                // Populate the TransportType dropdown
                ViewBag.FTransportType = new SelectList(db1.F_TRANSPORT_TYPES.ToList(), "ID", "TransportType");

                var invoice = db1.P_INVOICES.Where(x => x.IsDeleted == false && x.ID == invoiceId).FirstOrDefault();

                // Create a new PackingList model
                var model = new PackingListViewModel
                {
                    InvoiceID = invoiceId, // Use the invoiceId if provided, otherwise default to 0
                    Parts = new List<PackingListPart>() // Initialize the Parts list
                };

                // Get the parts associated with the invoice
                var invoiceParts = db1.P_INVOICE_PARTS.Where(x => x.InvoiceID == invoice.ID).ToList();

                // Populate the model.Parts list
                foreach (var invoicePart in invoiceParts)
                {
                    model.Parts.Add(new PackingListPart
                    {
                        PartID = invoicePart.PartID,
                        Part = invoicePart.PART
                    });
                }

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PackingListViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                double? TotalCBM = model.Parts.Sum(x => x.PrCBM);
                double? TotalGrWeight = model.Parts.Sum(x => x.PrGrWeight);
                double? TotalNetWeight = model.Parts.Sum(x => x.PrNetWeight);

                try
                {
                    /*if (ModelState.IsValid)
                    {*/
                    // Save the PackingList entity
                    var packingList = new P_INVOICE_PACKINGLISTS
                    {
                        InvoiceID = model.InvoiceID,
                        TransportNo = model.TransportNo,
                        TransportTypeID = model.TransportTypeID,
                        PackingListNo = model.PackingListNo,
                        SealNo = model.SealNo,
                        Comment = model.Comment,
                        InTransit = false,
                        TotalCBM = TotalCBM,
                        TotalGrWeight = TotalGrWeight,
                        TotalNetWeight = TotalNetWeight,

                        IsDeleted = false
                    };

                    db.P_INVOICE_PACKINGLISTS.Add(packingList);
                    db.SaveChanges();

                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "PackingListController", $"{packingList.ID} ID ga ega PackingList yaratdi");

                    var packinglistID = packingList.ID;

                    foreach (var item in model.Parts)
                    {
                        P_PACKINGLIST_PARTS newPart = new P_PACKINGLIST_PARTS
                        {
                            PackingListID = packinglistID,
                            PartID = item.PartID,
                            PieceWeight = db.PARTS.Where(p => p.ID == item.PartID).FirstOrDefault().PWeight,
                            PrLength = item.PrLength,
                            PrWidth = item.PrWidth,
                            PrHeight = item.PrHeight,
                            PrGrWeight = item.PrGrWeight,
                        };

                        db.P_PACKINGLIST_PARTS.Add(newPart);
                        LogHelper.LogToDatabase(userEmail, "PackingListController", $"{newPart.ID} ID ga ega PackingListPart yaratdi");
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index", "PInvoice");

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                // Retain the dropdown lists if there's an error
                ViewBag.Invoice = new SelectList(db.P_INVOICES.Where(p => p.IsDeleted == false).ToList(), "ID", "InvoiceNo", model.InvoiceID);
                ViewBag.FTransportType = new SelectList(db.F_TRANSPORT_TYPES.ToList(), "ID", "TransportType", model.TransportTypeID);
                //for (int i = 0; i < model.Parts.Count; i++)
                //{
                //    model.Parts[i].Part.PNo = db1.PARTS.Where(x => x.IsDeleted == false && x.ID == model.Parts[i].PartID).FirstOrDefault().PNo;
                //}
                return View(model);
            }
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
            using (DBTHSNEntities db = new DBTHSNEntities())
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

                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PackingListController", $"{ID} ID ga ega PackingListni o'chirdi");

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
                        }))
                        {
                            try
                            {
                                db.SaveChanges();

                                var userEmail = User.Identity.Name;
                                LogHelper.LogToDatabase(userEmail, "PackingListController", $"{packingList.ID} ID ga ega PackingListni tahrirladi");

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