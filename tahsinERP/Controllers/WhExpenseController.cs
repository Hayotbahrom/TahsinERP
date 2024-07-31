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
        public ActionResult Index(string type, int? supplierID)
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
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                WrhsExpenseViewModel viewModel = new WrhsExpenseViewModel();
                PART_WRHS_EXPENSES expense = db.PART_WRHS_EXPENSES.OrderByDescending(p => p.IssueDateTime).FirstOrDefault();
                var monthAndNumber = expense.DocNo.Split('_');

                if (int.Parse(monthAndNumber[0]) == int.Parse(DateTime.Now.Month.ToString()))
                {
                    int docNoNumber = int.Parse(monthAndNumber[1]) + 1;
                    viewModel.DocNo = DateTime.Now.Month + "_" + docNoNumber;
                }
                else
                    viewModel.DocNo = DateTime.Now.Month + "_" + 1;

                return View(viewModel);
            }
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
                    ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

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
                    SenderWHID = 1
                };

                db.PART_WRHS_EXPENSES.Add(newExpense);
                db.SaveChanges();

                //newExpense.SenderWHID = newExpense.ID;
                // Get the newly created ExpenseID
                int newExpenseID = newExpense.ID;
                newExpense.SenderWHID = newExpense.ID;
                db.Entry(newExpense).State = EntityState.Modified;
                //db.SaveChanges();
                // Save parts
                foreach (var part in model.Parts)
                {
                    PART_WRHS_EXPENSE_PARTS newPart = new PART_WRHS_EXPENSE_PARTS
                    {
                        ExpenseID = newExpenseID,
                        PartID = part.PartID,
                        UnitID = part.UnitID,
                        Amount = part.Amount,
                        PiecePrice = part.PiecePrice,
                        Comment = part.Comment
                    };

                    db.PART_WRHS_EXPENSE_PARTS.Add(newPart);
                }

                db.SaveChanges();
                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "WhExpenseController", "Create[Post]");
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
                                  .Include(pc => pc.UNIT)
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

            PART_WRHS_EXPENSES wrhsExpense;
            List<PART_WRHS_EXPENSE_PARTS> partList;

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                wrhsExpense = db1.PART_WRHS_EXPENSES
                    .FirstOrDefault(p => p.ID == ID);

                if (wrhsExpense == null)
                {
                    return HttpNotFound();
                }

                // suppliers = new SelectList(db1.SUPPLIERS.ToList(), "ID", "Name", wrhsExpense.SupplierID);

                partList = db1.PART_WRHS_EXPENSE_PARTS
                    .Include(whp => whp.PART)
                    .Include(pc => pc.UNIT)
                    .Where(whp => whp.ExpenseID == ID)
                    .ToList();
                //ViewBag.PartList = new SelectList(db1.PART_WRHS_EXPENSE_PARTS.Include(p => p.PART).ToList(),"ID","PName");
               // ViewBag.Invoices = new SelectList(db1.P_INVOICES.Where(i => i.IsDeleted == false).ToList(), "ID", "InvoiceNo");
                ViewBag.PartWhrs = new SelectList(db1.PART_WRHS.Where(i => i.IsDeleted == false).ToList(), "ID", "WHName");
                ViewBag.units = new SelectList(db1.UNITS.ToList(), "ID", "UnitName");

            }

            // ViewBag.Supplier = suppliers;
            ViewBag.partList = partList;

            return View(wrhsExpense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PART_WRHS_EXPENSES wrhsExpense)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db1 = new DBTHSNEntities())
                {
                    PART_WRHS_EXPENSES wrhsExpenseToUpdate = db1.PART_WRHS_EXPENSES.Find(wrhsExpense.ID);
                    if (wrhsExpenseToUpdate != null)
                    {
                        wrhsExpenseToUpdate.DocNo = wrhsExpense.DocNo;
                        wrhsExpenseToUpdate.ReceiverWhID = wrhsExpense.ReceiverWhID;
                        wrhsExpenseToUpdate.Currency = wrhsExpense.Currency;
                        wrhsExpenseToUpdate.Amount = wrhsExpense.Amount;
                        wrhsExpenseToUpdate.TotalPrice = wrhsExpense.TotalPrice;
                        wrhsExpenseToUpdate.Description = wrhsExpense.Description;
                        wrhsExpenseToUpdate.IssueDateTime = wrhsExpense.IssueDateTime;
                        wrhsExpenseToUpdate.IsDeleted = false;
                        //wrhsExpenseToUpdate.SenderWHID = wrhsExpense.SenderWHID;

                        try
                        {
                            db1.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "WhExpenseController", "Edit[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    return View(wrhsExpenseToUpdate);
                }
            }
            return View(wrhsExpense);
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
                var whExpensePart = db.PART_WRHS_EXPENSE_PARTS
                                    .Include(p => p.PART_WRHS_EXPENSES)
                                    .Include(p => p.PART)
                                    .Include(pc => pc.UNIT)
                                    .FirstOrDefault(p => p.ID == ID);
                if (whExpensePart == null)
                {
                    return HttpNotFound();
                }
                var allParts = db.PARTS
                                .Include(p => p.PART_WRHS_EXPENSE_PARTS)
                                .Include(pc => pc.UNIT)
                                .Select(p => new SelectListItem
                                {
                                    Value = p.ID.ToString(),
                                    Text = p.PNo
                                }).ToList();

                ViewBag.PartList = allParts;
                return View(whExpensePart);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPart(PART_WRHS_EXPENSE_PARTS whExpensePart)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PART_WRHS_EXPENSE_PARTS whExpensePartToUpdate = db.PART_WRHS_EXPENSE_PARTS.Find(whExpensePart.ID);
                    if (whExpensePartToUpdate != null)
                    {
                        whExpensePartToUpdate.PartID = whExpensePart.PartID;
                        whExpensePartToUpdate.Amount = whExpensePart.Amount;
                        whExpensePartToUpdate.UnitID = whExpensePart.UnitID;
                        whExpensePartToUpdate.PiecePrice = whExpensePart.PiecePrice;
                        whExpensePartToUpdate.Comment = whExpensePart.Comment;
                        //whExpensePartToUpdate.Amount = whExpensePart.Quantity * whExpensePart.Price; SQL o'zi chiqarib beradi

                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "WhExpenseController", "EditPart[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    return View(whExpensePartToUpdate);
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
                var wrhsExpense = db.PART_WRHS_EXPENSES.Find(Id);
                if (wrhsExpense == null)
                {
                    return HttpNotFound();
                }
                else
                    ViewBag.partList = db.PART_WRHS_EXPENSE_PARTS
                        .Include(pc => pc.PART)
                        .Include(pc => pc.UNIT)
                        .Where(pc => pc.ExpenseID == wrhsExpense.ID).ToList();

                db.Entry(wrhsExpense).Reference(i => i.PART_WRHS).Load();
                return View(wrhsExpense);
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
                    PART_WRHS_EXPENSES whExpenseToDelete = db.PART_WRHS_EXPENSES.Find(ID);
                    if (whExpenseToDelete != null)
                    {
                        whExpenseToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(whExpenseToDelete).State = System.Data.Entity.EntityState.Modified;
                            
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "WhExpenseController", "Delete[Post]");
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
                PART_WRHS_EXPENSE_PARTS whExpensePartToDelete = db.PART_WRHS_EXPENSE_PARTS.Find(id);
                if (ModelState.IsValid)
                {
                    if (whExpensePartToDelete != null)
                    {
                        try
                        {
                            db.PART_WRHS_EXPENSE_PARTS.Remove(whExpensePartToDelete);
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "WhExpenseController", "DeletePart[Post]");
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
                return View(whExpensePartToDelete);
            }
        }

    }
}
