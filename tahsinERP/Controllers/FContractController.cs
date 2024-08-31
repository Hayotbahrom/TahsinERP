using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class FContractController : Controller
    {
        // GET: FContract
        public async Task<ActionResult> Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = await db.F_CONTRACTS
                    .Include(fc => fc.FORWARDER)
                    .Where(fc => fc.IsDeleted == false)
                    .ToListAsync();

                return View(list);
            }
        }
        public async Task<ActionResult> Create()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Forwarder = new SelectList(await db.FORWARDERS.Where(fc => fc.IsDeleted == false).ToListAsync(), "ID", "ForwarderName");
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(F_CONTRACTS contract)
        {
            try
            {
                using(DBTHSNEntities db = new DBTHSNEntities()) 
                {
                    if (ModelState.IsValid)
                    {
                        contract.IsDeleted = false;
                        db.F_CONTRACTS.Add(contract);
                        await db.SaveChangesAsync();

                        var userEmail = User.Identity.Name;
                        LogHelper.LogToDatabase(userEmail, "FContractController", $"{contract.ContractNo} - FContractni yaratdi");

                        return RedirectToAction("Index"); 
                    }
                    else
                    {
                        ViewBag.Forwarder = new SelectList(await db.FORWARDERS.Where(fc => fc.IsDeleted == false).ToListAsync(), "ID", "ForwarderName");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }
            return View(contract);
        }
        public async Task<ActionResult> Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                var contract = await db1.F_CONTRACTS
                                  .Include(p => p.FORWARDER)
                                  .FirstOrDefaultAsync(p => p.ID == ID);

                if (contract == null)
                {
                    return HttpNotFound();
                }
                
                return View(contract);
            }
        }
        public async Task<ActionResult> Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                var contract = await db1.F_CONTRACTS
                    .Include(c => c.FORWARDER)
                    .FirstOrDefaultAsync(c => c.ID == ID);

                if (contract == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Forwarder = new SelectList( await db1.FORWARDERS.Where(f => f.IsDeleted == false).ToListAsync(), "ID", "ForwarderName");
                return View(contract);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(F_CONTRACTS contract)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var contractToUpdate = await db.F_CONTRACTS.FindAsync(contract.ID);
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
                            await db.SaveChangesAsync();

                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "DefectTypeController", $"{contract.ContractNo} - FContratni tahrirladi");

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
        public async Task<ActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contract = await db.F_CONTRACTS.FindAsync(Id);
                if (contract == null)
                {
                    return HttpNotFound();
                }

                await db.Entry(contract).Reference(i => i.FORWARDER).LoadAsync();
                return View(contract);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var contractToDelete = await db.F_CONTRACTS.FindAsync(ID);
                    if (contractToDelete != null)
                    {
                        contractToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(contractToDelete).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "DefectTypeController", $"{contractToDelete.ContractNo} - FContratni o'chirdi");

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