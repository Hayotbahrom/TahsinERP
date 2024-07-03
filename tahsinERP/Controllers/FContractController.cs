using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class FContractController : Controller
    {
        // GET: FContract
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = db.F_CONTRACTS
                    .Include(fc => fc.FORWARDER)
                    .Where(fc => fc.IsDeleted == false)
                    .ToList();

                return View(list);
            }
        }
        public ActionResult Create()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.FContract = new SelectList(db.F_CONTRACTS.Where(fc => fc.IsDeleted == false), "ID", "ContractNo");
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(F_CONTRACTS contract)
        {
            try
            {
                using(DBTHSNEntities db = new DBTHSNEntities())
                {
                    if (ModelState.IsValid)
                    {
                        db.F_CONTRACTS.Add(contract);
                        db.SaveChanges();

                        return RedirectToAction("Index"); 
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }
            return View(contract);
        }
        public ActionResult Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                var contract = db1.F_CONTRACTS
                                  .Include(p => p.FORWARDER)
                                  .FirstOrDefault(p => p.ID == ID);

                if (contract == null)
                {
                    return HttpNotFound();
                }
                
                return View(contract);
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
                var contract = db1.F_CONTRACTS
                    .Include(c => c.FORWARDER)
                    .FirstOrDefault(c => c.ID == ID);

                if (contract == null)
                {
                    return HttpNotFound();
                }

                return View(contract);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(F_CONTRACTS contract)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db1 = new DBTHSNEntities())
                {
                    var contractToUpdate = db1.F_CONTRACTS.Find(contract.ID);
                    if (contractToUpdate != null)
                    {
                        contractToUpdate.ContractNo = contract.ContractNo;
                        contractToUpdate.ForwarderID = contract.ForwarderID;
                        contractToUpdate.IssueDate = contract.IssueDate;
                        contractToUpdate.DueDate = contract.DueDate;
                        contractToUpdate.Currency = contract.Currency;
                        contractToUpdate.Amount = contract.Amount;
                        contractToUpdate.Description = contract.Description;
                        contractToUpdate.IsDeleted = false;

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
                    return View(contractToUpdate);
                }
            }
            return View(contract);
        }
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contract = db.F_CONTRACTS.Find(Id);
                if (contract == null)
                {
                    return HttpNotFound();
                }

                db.Entry(contract).Reference(i => i.FORWARDER).Load();
                return View(contract);
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
                    var contractToDelete = db.F_CONTRACTS.Find(ID);
                    if (contractToDelete != null)
                    {
                        contractToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(contractToDelete).State = System.Data.Entity.EntityState.Modified;
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