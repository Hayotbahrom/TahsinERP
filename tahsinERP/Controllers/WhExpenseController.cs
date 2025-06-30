using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class WhExpenseController : Controller
    {
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
        PART_WRHS warehouse = null;
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
        private PART_WRHS GetWarehouseOfMRP(string email)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                USER currentUser = db.USERS.Where(u => u.Email.CompareTo(email) == 0 && u.IsDeleted == false && u.IsActive == true).FirstOrDefault();
                PART_WRHS his_warehouse = db.PART_WRHS.Where(wrhs => wrhs.MRP == currentUser.ID).FirstOrDefault();

                return his_warehouse;
            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var partInStock = from part in db.PARTS
                                  join stock in db.PART_STOCKS on part.ID equals stock.PartID
                                  where part.IsDeleted == false
                                  select part;

                // Populate dropdown lists
                ViewBag.PartWrhs = new SelectList(db.PART_WRHS.Where(w => w.IsDeleted == false).ToList(), "ID", "WHName");
                ViewBag.InComes = new SelectList(db.PART_WRHS_EXPENSES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
                ViewBag.InComeParts = new SelectList(partInStock.ToList(), "ID", "PNo");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                // Initialize view model
                WrhsExpenseViewModel viewModel = new WrhsExpenseViewModel();
                PART_WRHS_EXPENSES expense = db.PART_WRHS_EXPENSES.OrderByDescending(p => p.IssueDateTime).FirstOrDefault();

                warehouse = GetWarehouseOfMRP(User.Identity.Name);
                if (warehouse is null)
                    viewModel.WHName = "Siz uchun ombor biriktirilmagan.";
                else
                    viewModel.WHName = warehouse.WHName;
                if (expense == null)
                {
                    viewModel.DocNo = DateTime.Now.Month + "_" + 1;
                }
                else
                {
                    var monthAndNumber = expense.DocNo.Split('_');
                    if (int.Parse(monthAndNumber[0]) == DateTime.Now.Month)
                    {
                        int docNoNumber = int.Parse(monthAndNumber[1]) + 1;
                        viewModel.DocNo = DateTime.Now.Month + "_" + docNoNumber;
                    }
                    else
                    {
                        viewModel.DocNo = DateTime.Now.Month + "_" + 1;
                    }
                }

                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Create(WrhsExpenseViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (!ModelState.IsValid)
                {
                    PopulateViewBags(db);
                    return View(model);
                }
                // Validate ReceiverWhID exists
                var receiverWarehouse = db.PART_WRHS.FirstOrDefault(w => w.ID == model.RecieverWHID && w.IsDeleted == false);
                if (receiverWarehouse == null)
                {
                    ModelState.AddModelError("RecieverWHID", "The selected warehouse does not exist.");
                    PopulateViewBags(db);
                    return View(model);
                }

                // Create a new PART_WRHS_EXPENSES record
                PART_WRHS_EXPENSES newExpense = new PART_WRHS_EXPENSES
                {
                    DocNo = model.DocNo,
                    ReceiverWhID = model.RecieverWHID,
                    Currency = model.Currency,
                    IsDeleted = false,
                    Description = model.Description,
                    IssueDateTime = DateTime.Now,
                    SendStatus = true,
                    SenderWHID = 1 // Assuming SenderWHID is always 1
                };

                db.PART_WRHS_EXPENSES.Add(newExpense);
                db.SaveChanges();

                // Update SenderWHID with the newly created ExpenseID
                newExpense.SenderWHID = GetWarehouseOfMRP(User.Identity.Name).ID;
                db.Entry(newExpense).State = EntityState.Modified;
                db.SaveChanges();

                LogHelper.LogToDatabase(User.Identity.Name, "WhExpenseController", $"{newExpense.DocNo} - PartWrhsExpenseni yaratdi");

                // Save parts
                foreach (var part in model.Parts)
                {
                    var existStock = db.PART_STOCKS.FirstOrDefault(x => x.PartID == part.PartID);
                    if (existStock == null)
                    {
                        PopulateViewBags(db);
                        ModelState.AddModelError("", "Omborda bunday ehtiyot qism topilmadi.");
                        return View(model);
                    }
                    else if (existStock.Amount >= part.Amount)
                    {
                        existStock.Amount = existStock.Amount - part.Amount;
                        db.SaveChanges();
                    }
                    else
                    {
                        PopulateViewBags(db);
                        ModelState.AddModelError("", "Kiritilgan miqdor CHIQIM qilish uchun ombordagi bor imkoniyatdan oshib ketdi.");
                        return View(model);
                    }
                }

                return RedirectToAction("Index");
            }
        }
        private void PopulateViewBags(DBTHSNEntities db)
        {
            var partInStock = from part in db.PARTS
                              join stock in db.PART_STOCKS on part.ID equals stock.PartID
                              where part.IsDeleted == false
                              select part;

            ViewBag.PartWrhs = new SelectList(db.PART_WRHS.Where(w => w.IsDeleted == false).ToList(), "ID", "WHName");
            ViewBag.InComes = new SelectList(db.PART_WRHS_EXPENSES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
            ViewBag.InComeParts = new SelectList(partInStock.ToList(), "ID", "PNo");
            ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
        }
        public ActionResult Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var wrhsExpense = db.PART_WRHS_EXPENSES
                                  .Include(x => x.PART_WRHS)
                                  .FirstOrDefault(p => p.ID == ID);

                if (wrhsExpense == null)
                {
                    return HttpNotFound();
                }

                var partList = db.PART_WRHS_EXPENSE_PARTS
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

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                wrhsExpense = db.PART_WRHS_EXPENSES
                    .FirstOrDefault(p => p.ID == ID);

                if (wrhsExpense == null)
                {
                    return HttpNotFound();
                }

                // suppliers = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name", wrhsExpense.SupplierID);

                partList = db.PART_WRHS_EXPENSE_PARTS
                    .Include(whp => whp.PART)
                    .Include(pc => pc.UNIT)
                    .Where(whp => whp.ExpenseID == ID)
                    .ToList();
                //ViewBag.PartList = new SelectList(db.PART_WRHS_EXPENSE_PARTS.Include(p => p.PART).ToList(),"ID","PName");
               // ViewBag.Invoices = new SelectList(db.P_INVOICES.Where(i => i.IsDeleted == false).ToList(), "ID", "InvoiceNo");
                ViewBag.PartWhrs = new SelectList(db.PART_WRHS.Where(i => i.IsDeleted == false).ToList(), "ID", "WHName");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

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
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PART_WRHS_EXPENSES wrhsExpenseToUpdate = db.PART_WRHS_EXPENSES.Find(wrhsExpense.ID);
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
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "WhExpenseController", $"{wrhsExpenseToUpdate.DocNo} - PartWrhsExpenseni tahrirladi");

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

                            LogHelper.LogToDatabase(User.Identity.Name, "WhExpenseController", $"{whExpensePartToUpdate.PART.PNo} - PartWrhsExpensePartni tahrirladi");

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

                            LogHelper.LogToDatabase(User.Identity.Name, "WhExpenseController", $"{whExpenseToDelete.DocNo} - PartWrhsExpenseni o'chirdi");

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

                            LogHelper.LogToDatabase(User.Identity.Name, "WhExpenseController", $"{whExpensePartToDelete.PART.PNo} - PartWrhsExpensePartni o'chirdi");

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
        public async Task<ActionResult> Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES invoys = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("ombor_chiqim.xlsx") == 0).FirstOrDefault();
                if (invoys != null)
                    return File(invoys.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                return View();
            }
        }

        public ActionResult UploadWithExcel()
        {
            ViewBag.IsFileUploaded = false;
            return View();
        }

        [HttpPost]
        public ActionResult UploadWithExcel(HttpPostedFileBase file)
        {
            // Process uploaded file
            // ...
            return View();
        }
    }
}
