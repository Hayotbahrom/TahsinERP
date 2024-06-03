using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
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
            ViewBag.Companies = new SelectList(db.COMPANIES.ToList(), "CompanyID", "CompanyName");
            ViewBag.Suppliers = new SelectList(db.SUPPLIERS.ToList(), "SupplierID", "SupplierName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContractNo, IssuedDate, CompanyID, SupplierID, PartID, Price, Currency, Amount, Incoterms, PaymentTerms, MOQ,MaximumCapacity, Unit,DueDate")] P_CONTRACTS contract)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
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

        public ActionResult Details(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var contract = db.P_CONTRACTS.Find(Id);
            if (contract == null)
            {
                return HttpNotFound();
            }

            return View(contract);
            //return RedirectToAction("SupplierParts?supplierId="+supplier.ID);
        }

        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var contract = db.P_CONTRACTS.Find(Id);
            if (contract == null)
            {
                return HttpNotFound();
            }

            return View(contract);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(P_CONTRACTS contract)
        {
            if (ModelState.IsValid)
            {
                P_CONTRACTS contractToUpdate = db.P_CONTRACTS.Find(contract.ID);
                if (contractToUpdate != null)
                {
                    contractToUpdate.IssuedDate = contract.IssuedDate;
                    contractToUpdate.ContractNo = contract.ContractNo;
                    contractToUpdate.CompanyID = contract.CompanyID;
                    contractToUpdate.SupplierID = contract.SupplierID;
                    contractToUpdate.PartID = contract.PartID;
                    contractToUpdate.Price = contract.Price;
                    contractToUpdate.Currency = contract.Currency;
                    contractToUpdate.Amount = contract.Amount;
                    contractToUpdate.Incoterms = contract.Incoterms;
                    contractToUpdate.PaymentTerms = contract.PaymentTerms;
                    contractToUpdate.MOQ = contract.MOQ;
                    contractToUpdate.MaximumCapacity = contract.MaximumCapacity;
                    contractToUpdate.Unit = contract.Unit;
                    contractToUpdate.DueDate = contract.DueDate;
                    
                    if (TryUpdateModel(contractToUpdate, "", new string[] { "ContractNo, IssuedDate, CompanyID, SupplierID, PartID, Price, Currency, Amount, Incoterms, PaymentTerms, MOQ,MaximumCapacity, Unit,DueDate" }))
                    {
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                        }
                    }
                }
                return View(contractToUpdate);
            }
            return View();
        }

        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var contract = db.P_CONTRACTS.Find(Id);
            if (contract == null)
            {
                return HttpNotFound();
            }

            return View(contract);
            //return RedirectToAction("SupplierParts?supplierId="+supplier.ID);
        }

        // Delete Shartnomani to'liq o'chirish qismi bo'ldi, ba'zi detallarini o'chirishga to'liq qilingani yo'q
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                P_CONTRACTS contractToDelete = db.P_CONTRACTS.Find(ID);
                if (contractToDelete != null)
                {
                    try
                    {
                        db.P_CONTRACTS.Remove(contractToDelete);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (RetryLimitExceededException)
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Contract not found.");
                }
            }
            return View();
        }


    }
}