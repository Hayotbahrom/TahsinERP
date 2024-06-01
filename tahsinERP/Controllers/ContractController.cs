using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ContractController : Controller
    {

        private DBTHSNEntities db = new DBTHSNEntities();
        private string[] sources = new string[3] { "", "Import", "Lokal" };

        
        // GET: Contracts
        public ActionResult Index(string type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                List<P_CONTRACTS> list = db.P_CONTRACTS.ToList();
                ViewBag.SourceList = new SelectList(sources, type);
                return View(list);
            }
            else
            {
                List<P_CONTRACTS> list = db.P_CONTRACTS.ToList();
                ViewBag.SourceList = new SelectList(sources, type);
                return View(list);
            }
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContractNo, CompanyID, SupplierID, PartID, Price, Currency, Amount, Incoterms, PaymentTerms, MOQ,MaximumCapacity, Unit,DueDate")] P_CONTRACTS contract)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   // contract.IssuedDate = DateTime.UtcNow;
                    db.P_CONTRACTS.Add(contract);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }
            return View(contract);
        }
    }
}