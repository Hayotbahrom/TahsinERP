using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

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
            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                ViewBag.Contract = new SelectList(db1.F_CONTRACTS.ToList(), "ID", "ContractNo");
                ViewBag.TransportType = new SelectList(db1.F_TRANSPORT_TYPES.ToList(), "ID", "TransportType");
                ViewBag.Invoice = new SelectList(db1.P_INVOICES.ToList(), "ID", "InvoiceNo");
                ViewBag.PackingList = new SelectList(db1.P_INVOICE_PACKINGLISTS.ToList(), "ID", "PackingListNo");

                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WaybillNo, ContractID, TransportTypeID, InvoiceID, PackingListID, CBM, GrWeight, Description, IsDeleted")] F_WAYBILLS waybill)
        {

            try
            {
                using (DBTHSNEntities db1 = new DBTHSNEntities())
                {
                    if (ModelState.IsValid)
                    {
                        db1.F_WAYBILLS.Add(waybill);
                        db1.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }
            return View(waybill);
        }


    }
}