using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class WaybillController : Controller
    {
        // GET: Waybill
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = db.F_WAYBILLS
                    .Include( w => w.F_CONTRACTS)
                    .Include(w => w.P_INVOICES)
                    .Include(w => w.F_TRANSPORT_TYPES)
                    .Where(w => w.IsDeleted == false).ToList();

                return View(list);
            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var viewModel = new WaybillCreateViewModel
                {
                    Contracts = db.F_CONTRACTS.Select(c => new SelectListItem
                    {
                        Value = c.ID.ToString(),
                        Text = c.ContractNo
                    }).ToList(),

                    TransportTypes = db.F_TRANSPORT_TYPES.Select(t => new SelectListItem
                    {
                        Value = t.ID.ToString(),
                        Text = t.TransportType
                    }).ToList(),

                    Invoices = db.P_INVOICES.Select(i => new SelectListItem
                    {
                        Value = i.ID.ToString(),
                        Text = i.InvoiceNo
                    }).ToList(),

                    PackingLists = db.P_INVOICE_PACKINGLISTS.Select(p => new SelectListItem
                    {
                        Value = p.ID.ToString(),
                        Text = p.PackingListNo
                    }).ToList()
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WaybillCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var waybill = new F_WAYBILLS
                    {
                        WaybillNo = viewModel.WaybillNo,
                        ContractID = viewModel.ContractID,
                        TransportTypeID = viewModel.TransportTypeID,
                        InvoiceID = viewModel.InvoiceID,
                        CBM = viewModel.CBM,
                        GrWeight = viewModel.GrWeight,
                        Description = viewModel.Description,
                        IsDeleted = viewModel.IsDeleted
                    };

                    db.F_WAYBILLS.Add(waybill);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            // Repopulate dropdown lists if validation fails
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                viewModel.Contracts = db.F_CONTRACTS.Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),
                    Text = c.ContractNo
                }).ToList();

                viewModel.TransportTypes = db.F_TRANSPORT_TYPES.Select(t => new SelectListItem
                {
                    Value = t.ID.ToString(),
                    Text = t.TransportType
                }).ToList();

                viewModel.Invoices = db.P_INVOICES.Select(i => new SelectListItem
                {
                    Value = i.ID.ToString(),
                    Text = i.InvoiceNo
                }).ToList();

                viewModel.PackingLists = db.P_INVOICE_PACKINGLISTS.Select(p => new SelectListItem
                {
                    Value = p.ID.ToString(),
                    Text = p.PackingListNo
                }).ToList();
            }

            return View(viewModel);
        }
        public ActionResult Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var waybill = db.F_WAYBILLS
                                .Include(w => w.F_CONTRACTS)
                                .Include(w => w.P_INVOICES)
                                .Include(w => w.F_TRANSPORT_TYPES)
                                .FirstOrDefault(w => w.ID == ID);

                if (waybill == null)
                {
                    return HttpNotFound();
                }

                return View(waybill);
            }
        }
        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                var waybill = db1.F_WAYBILLS
                                .Include(w => w.F_CONTRACTS)
                                .Include(w => w.P_INVOICES)
                                .Include(w => w.F_TRANSPORT_TYPES)
                                .FirstOrDefault(w => w.ID == ID);

                if (waybill == null)
                {
                    return HttpNotFound();
                };
                ViewBag.Invoice = new SelectList(db1.P_INVOICES.Where(fc => fc.IsDeleted == false).ToList(), "ID", "InvoiceNo");
                ViewBag.Contract = new SelectList(db1.F_CONTRACTS.Where(fc => fc.IsDeleted == false).ToList(), "ID", "ContractNo");
                ViewBag.TransportType = new SelectList(db1.F_TRANSPORT_TYPES.ToList(), "ID", "TransportType");
                ViewBag.PackingList = new SelectList(db1.P_INVOICE_PACKINGLISTS.Where(fc => fc.IsDeleted == false).ToList(), "ID", "PackingListNo");

                return View(waybill);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(F_WAYBILLS waybill)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db1 = new DBTHSNEntities())
                {
                    var  waybillToUpdate = db1.F_WAYBILLS.Find(waybill.ID);
                    if (waybillToUpdate != null)
                    {
                        waybillToUpdate.WaybillNo = waybill.WaybillNo;
                        waybillToUpdate.ContractID = waybill.ContractID;
                        waybillToUpdate.TransportTypeID = waybill.TransportTypeID;
                        waybillToUpdate.InvoiceID = waybill.InvoiceID;
                        waybillToUpdate.CBM = waybill.CBM;
                        waybillToUpdate.GrWeight = waybill.GrWeight;
                        waybillToUpdate.Description = waybill.Description;
                        waybillToUpdate.IsDeleted = false;

                        try
                        {
                            db1.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    return View(waybillToUpdate);
                }
            }
            return View(waybill);
        }
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var waybill = db.F_WAYBILLS.Find(Id);
                if (waybill == null)
                {
                    return HttpNotFound();
                }
                else
                    ViewBag.partList = db.P_CONTRACT_PARTS
                        .Include(pc => pc.PART)
                        .Where(pc => pc.ContractID == waybill.ID).ToList();

                db.Entry(waybill).Reference(i => i.F_CONTRACTS).Load();
                db.Entry(waybill).Reference(i => i.F_TRANSPORT_TYPES).Load();
                db.Entry(waybill).Reference(i => i.P_INVOICES).Load();

                return View(waybill);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var waybillToDelete = db.F_WAYBILLS.Find(ID);
                    if (waybillToDelete != null)
                    {
                        waybillToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(waybillToDelete).State = System.Data.Entity.EntityState.Modified;
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


    }
}