using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PInvoiceController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private readonly string[] sources = new string[4] { "", "KD", "Steel", "Maxalliy" };
        // GET: PInvoice
        public ActionResult Index(string type,  int? supplierID)
        {
            if (!string.IsNullOrEmpty(type))
            {
                if (supplierID.HasValue)
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false && pi.SUPPLIER.Type.CompareTo(type) == 0 && pi.SupplierID == supplierID).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name", supplierID);

                    return View(list);
                }
                else
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false && pi.SUPPLIER.Type.CompareTo(type) == 0).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name");

                    return View(list);
                }
            }
            else
            {
                if (supplierID.HasValue)
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false && pi.SupplierID == supplierID).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", supplierID);

                    return View(list);
                }
                else
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false ).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name");

                    return View(list);
                }
            }
        }
        public ActionResult Create()
        {
            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name");
            ViewBag.POrder = new SelectList(db.P_ORDERS, "ID", "ContractNo");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InvoiceNo, CompanyID, SupplierID, OrderID, InvoiceDate, Amount, Currency")] P_INVOICES invoice)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    invoice.IsDeleted = false;
                    db.P_INVOICES.Add(invoice); 

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }

            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name", invoice.SupplierID);
            ViewBag.POrder = new SelectList(db.P_CONTRACTS, "ID", "ContractNo", invoice.OrderID);

            return View(invoice);
        }
    }
}