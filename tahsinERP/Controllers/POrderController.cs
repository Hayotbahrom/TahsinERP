using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class POrderController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();

        // GET: POrder
        public ActionResult Index(string type)
        {
            var sources = new string[3] { "", "Import", "Lokal" };
            List<P_ORDERS> list;
            if (!string.IsNullOrEmpty(type))
            {
                list = db.P_ORDERS.Where(po => po.IsDeleted == false && po.SUPPLIER.Type.CompareTo(type) == 0).ToList();
                ViewBag.SourceList = new SelectList(sources, type);
            }
            else
            {
                list = db.P_ORDERS.Where(po => po.IsDeleted == false).ToList();
                ViewBag.SourceList = new SelectList(sources, "");  // Set default selection to empty string
            }
            return View(list);
        }
        // GET: POrder/Create
        public ActionResult Create()
        {
            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name");
            ViewBag.PContract = new SelectList(db.P_CONTRACTS, "ID", "ContractNo");

            return View();
        }
        // POST: POrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderNo, IssuedDate, CompanyID, SupplierID, ContractID, Amount, Currency, Description")] P_ORDERS order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    order.IsDeleted = false;
                    db.P_ORDERS.Add(order);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }

            // Re-populate the ViewBag data to ensure dropdown lists are available in case of an error
            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name", order.SupplierID);
            ViewBag.PContract = new SelectList(db.P_CONTRACTS, "ID", "ContractNo", order.ContractID);
            return View(order);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var order = db.P_ORDERS.Find(id);
            if (order == null)
                return HttpNotFound();


            return View();
        }
    }
}
