using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class WhExpenseController : Controller
    {
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');

        // GET: WhExpense
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = db.PART_WRHS_EXPENSES
                    .Include(pe => pe.PART_WRHS)
                    .Where(pe => pe.IsDeleted == false)
                    .ToList();
                return View(list);
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.PartWrhs = new SelectList(db.PART_WRHS.Where(w => w.IsDeleted == false).ToList(), "ID", "WHName");
                ViewBag.InComes = new SelectList(db.PART_WRHS_EXPENSES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
                ViewBag.InComeParts = new SelectList(db.PARTS.Where(c => c.IsDeleted == false).ToList(), "ID", "PNo");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(WrhsExpenseViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Validate ReceiverWhID exists
                var receiverWarehouse = db.PART_WRHS.FirstOrDefault(w => w.ID == model.RecieverWHID && w.IsDeleted == false);
                if (receiverWarehouse == null)
                {
                    ModelState.AddModelError("ResieverWHID", "The selected warehouse does not exist.");
                    ViewBag.PartWrhs = new SelectList(db.PART_WRHS.Where(w => w.IsDeleted == false).ToList(), "ID", "WHName");
                    ViewBag.InComes = new SelectList(db.PART_WRHS_EXPENSES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
                    ViewBag.InComeParts = new SelectList(db.PARTS.Where(c => c.IsDeleted == false).ToList(), "ID", "PNo");
                    return View(model);
                }

                // Create a new PART_WRHS_EXPENSES record
                PART_WRHS_EXPENSES newExpense = new PART_WRHS_EXPENSES
                {
                    DocNo = model.DocNo,
                    ReceiverWhID = model.RecieverWHID,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    IsDeleted = false,
                    Description = model.Description,
                    IssueDateTime = DateTime.Now,
                    SendStatus = model.SendStatus,
                    SenderWHID = 0
                };

                db.PART_WRHS_EXPENSES.Add(newExpense);
                db.SaveChanges();

                //newExpense.SenderWHID = newExpense.ID;
                // Get the newly created ExpenseID
                int newExpenseID = newExpense.ID;
                newExpense.SenderWHID = newExpense.ID;
                db.Entry(newExpense).State = EntityState.Modified;
                db.SaveChanges();
                // Save parts
                foreach (var part in model.Parts)
                {
                    PART_WRHS_EXPENSE_PARTS newPart = new PART_WRHS_EXPENSE_PARTS
                    {
                        ExpenseID = newExpenseID,
                        PartID = part.PartID,
                        Unit = part.Unit,
                        Amount = part.Amount,
                        PiecePrice = part.PiecePrice,
                        Comment = part.Comment
                    };

                    db.PART_WRHS_EXPENSE_PARTS.Add(newPart);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        public ActionResult Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                var wrhsExpense = db1.PART_WRHS_EXPENSES
                                  .Include(x => x.PART_WRHS)
                                  .FirstOrDefault(p => p.ID == ID);

                if (wrhsExpense == null)
                {
                    return HttpNotFound();
                }

                var partList = db1.PART_WRHS_EXPENSE_PARTS
                                  .Include(pc => pc.PART)
                                  .Where(pc => pc.ExpenseID == wrhsExpense.ID)
                                  .ToList();

                ViewBag.PartList = partList;

                return View(wrhsExpense);
            }
        }

        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PART_WRHS_EXPENSES whrsIncome;
            List<PART_WRHS_EXPENSE_PARTS> partList;

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                whrsIncome = db1.PART_WRHS_EXPENSES
                    .FirstOrDefault(p => p.ID == ID);

                if (whrsIncome == null)
                {
                    return HttpNotFound();
                }

                // suppliers = new SelectList(db1.SUPPLIERS.ToList(), "ID", "Name", whrsIncome.SupplierID);

                partList = db1.PART_WRHS_EXPENSE_PARTS
                    .Where(whp => whp.ExpenseID == ID)
                    .Include(whp => whp.PART)
                    .ToList();
                //ViewBag.PartList = new SelectList(db1.PART_WRHS_INCOME_PARTS.Include(p => p.PART).ToList(),"ID","PName");
               // ViewBag.Invoices = new SelectList(db1.P_INVOICES.Where(i => i.IsDeleted == false).ToList(), "ID", "InvoiceNo");
                ViewBag.ParWhrs = new SelectList(db1.PART_WRHS.Where(i => i.IsDeleted == false).ToList(), "ID", "WHName");
            }

            // ViewBag.Supplier = suppliers;
            ViewBag.partList = partList;

            return View(whrsIncome);
        }
        /*

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PART_WRHS_EXPENSES whrsIncome)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db1 = new DBTHSNEntities())
                {
                    PART_WRHS_EXPENSES wrhsIncomeToUpdate = db1.PART_WRHS_EXPENSES.Find(whrsIncome.ID);
                    if (wrhsIncomeToUpdate != null)
                    {
                        wrhsIncomeToUpdate.DocNo = whrsIncome.DocNo;
                        // wrhsIncomeToUpdate.WHID = whrsIncome.WHID;
                        wrhsIncomeToUpdate.InvoiceID = whrsIncome.InvoiceID;
                        wrhsIncomeToUpdate.WaybillID = whrsIncome.WaybillID;
                        wrhsIncomeToUpdate.Currency = whrsIncome.Currency;
                        wrhsIncomeToUpdate.Amount = whrsIncome.Amount;
                        wrhsIncomeToUpdate.TotalPrice = whrsIncome.TotalPrice;
                        wrhsIncomeToUpdate.Description = whrsIncome.Description;
                        wrhsIncomeToUpdate.IssueDateTime = whrsIncome.IssueDateTime;
                        //wrhsIncomeToUpdate.SenderWHID = whrsIncome.SenderWHID;

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
                    return View(wrhsIncomeToUpdate);
                }
            }
            return View(whrsIncome);
        }
        //hali toliq ozgartitilmagan

        public ActionResult EditPart(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var whIncomePart = db.PART_WRHS_INCOME_PARTS
                                    .Include(p => p.PART_WRHS_EXPENSES)
                                    .Include(p => p.PART)
                                    .FirstOrDefault(p => p.ID == ID);
                if (whIncomePart == null)
                {
                    return HttpNotFound();
                }
                var allParts = db.PARTS
                                .Include(p => p.PART_WRHS_INCOME_PARTS)
                                .Select(p => new SelectListItem
                                {
                                    Value = p.ID.ToString(),
                                    Text = p.PNo
                                }).ToList();

                ViewBag.PartList = allParts;

                return View(whIncomePart);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPart(PART_WRHS_INCOME_PARTS whIncomePart)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PART_WRHS_INCOME_PARTS whIncomePartToUpdate = db.PART_WRHS_INCOME_PARTS.Find(whIncomePart.ID);
                    if (whIncomePartToUpdate != null)
                    {
                        whIncomePartToUpdate.PartID = whIncomePart.PartID;
                        whIncomePartToUpdate.Amount = whIncomePart.Amount;
                        whIncomePartToUpdate.Unit = whIncomePart.Unit;
                        whIncomePartToUpdate.PiecePrice = whIncomePart.PiecePrice;
                        whIncomePartToUpdate.Comment = whIncomePart.Comment;
                        //whIncomePartToUpdate.Amount = whIncomePart.Quantity * whIncomePart.Price; SQL o'zi chiqarib beradi

                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    return View(whIncomePartToUpdate);
                }
            }
            return View();
        }
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var whrsIncome = db.PART_WRHS_EXPENSES.Find(Id);
                if (whrsIncome == null)
                {
                    return HttpNotFound();
                }
                else
                    ViewBag.partList = db.PART_WRHS_INCOME_PARTS
                        .Include(pc => pc.PART)
                        .Where(pc => pc.IncomeID == whrsIncome.ID).ToList();

                db.Entry(whrsIncome).Reference(i => i.P_INVOICES).Load();
                db.Entry(whrsIncome).Reference(i => i.F_WAYBILLS).Load();
                return View(whrsIncome);
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
                    PART_WRHS_EXPENSES whIncomeToDelete = db.PART_WRHS_EXPENSES.Find(ID);
                    if (whIncomeToDelete != null)
                    {
                        whIncomeToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(whIncomeToDelete).State = System.Data.Entity.EntityState.Modified;
                            
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
        public ActionResult DeletePart(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PART_WRHS_INCOME_PARTS contractPartToDelete = db.PART_WRHS_INCOME_PARTS.Find(id);
                if (ModelState.IsValid)
                {
                    if (contractPartToDelete != null)
                    {
                        try
                        {
                            db.PART_WRHS_INCOME_PARTS.Remove(contractPartToDelete);
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
                        ModelState.AddModelError("", "ContractPart not found.");
                    }
                }
                return View(contractPartToDelete);
            }
        }*/

    }
}
