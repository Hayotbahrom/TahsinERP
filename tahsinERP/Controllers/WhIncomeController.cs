using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class WhIncomeController : Controller
    {
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
        //private string partNo, waybillNo, whName, docNo, invoiceNo = "";

        // GET: WhIncome
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (!string.IsNullOrEmpty(type))
                {
                    if (supplierID.HasValue)
                    {
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES
                            .Include(pr => pr.P_INVOICES)
                            .Include(pr => pr.F_WAYBILLS)
                            .Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0 && pi.P_INVOICES.SupplierID == supplierID)
                            .ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES
                            .Include(pr => pr.P_INVOICES)
                            .Include(pr => pr.F_WAYBILLS)
                            .Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0)
                            .ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES
                            .Include(pr => pr.P_INVOICES)
                            .Include(pr => pr.F_WAYBILLS)
                            .Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SupplierID == supplierID)
                            .ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES
                            .Include(pr => pr.P_INVOICES)
                            .Include(pr => pr.F_WAYBILLS)
                            .Where(pi => pi.IsDeleted == false)
                            .ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                //ViewBag.Wrhs = new SelectList(db.PART_WRHS.Where(w => w.IsDeleted == false).ToList(), "ID", "WHName");
                ViewBag.Invoices = new SelectList(db.P_INVOICES.Where(i => i.IsDeleted == false).ToList(), "ID", "InvoiceNo");
                ViewBag.Waybills = new SelectList(db.F_WAYBILLS.Where(w => w.IsDeleted == false).ToList(), "ID", "WaybillNo");

                ViewBag.InComes = new SelectList(db.PART_WRHS_INCOMES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
                ViewBag.InComeParts = new SelectList(db.PARTS.Where(c => c.IsDeleted == false).ToList(), "ID", "PNo");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(WrhsIncomeViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Yangi PART_WRHS_INCOMES yozuvini yaratish
                PART_WRHS_INCOMES newIncome = new PART_WRHS_INCOMES
                {
                    DocNo = model.DocNo,
                    WHID = null,
                    InvoiceID = model.InvoiceID,
                    WaybillID = model.WaybillID,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    IsDeleted = false,
                    Description = model.Description,
                    IssueDateTime = DateTime.Now,
                    SenderWHID = null,
                    RecieveStatus = model.RecieveStatus,
                };

                db.PART_WRHS_INCOMES.Add(newIncome);
                db.SaveChanges();

                // Yangi yozuvning IncomeID sini olish
                int newIncomeID = newIncome.ID;

                // Parts ni saqlash
                foreach (var part in model.Parts)
                {
                    PART_WRHS_INCOME_PARTS newPart = new PART_WRHS_INCOME_PARTS
                    {
                        IncomeID = newIncomeID, // part.IncomeID emas, yangi yaratilgan IncomeID ishlatiladi
                        PartID = part.PartID,
                        Unit = part.Unit,
                        Amount = part.Amount,
                        PiecePrice = part.PiecePrice,
                        //TotalPrice = part.TotalPrice,
                        Comment = part.Comment
                    };

                    db.PART_WRHS_INCOME_PARTS.Add(newPart);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }


        public async Task<ActionResult> Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES invoys = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("ombor_kirim.xlsx") == 0).FirstOrDefault();
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

        public class RepeaterItem
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public int WrhsID { get; set; }
        }

        public ActionResult Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                var wrhsIncome = db1.PART_WRHS_INCOMES
                                  .Include(p => p.P_INVOICES)
                                  .Include(p => p.F_WAYBILLS)
                                  .FirstOrDefault(p => p.ID == ID);

                if (wrhsIncome == null)
                {
                    return HttpNotFound();
                }

                var partList = db1.PART_WRHS_INCOME_PARTS
                                  .Include(pc => pc.PART)
                                  .Where(pc => pc.IncomeID == wrhsIncome.ID)
                                  .ToList();

                ViewBag.PartList = partList;

                return View(wrhsIncome);
            }
        }

        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PART_WRHS_INCOMES whrsIncome;
            List<PART_WRHS_INCOME_PARTS> partList;
            SelectList suppliers;

            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                whrsIncome = db1.PART_WRHS_INCOMES
                    .Include(i => i.P_INVOICES)
                    .Include(i => i.F_WAYBILLS)
                    .FirstOrDefault (p => p.ID == ID);

                if (whrsIncome == null)
                {
                    return HttpNotFound();
                }

               // suppliers = new SelectList(db1.SUPPLIERS.ToList(), "ID", "Name", whrsIncome.SupplierID);

                partList = whrsIncome.PART_WRHS_INCOME_PARTS.ToList();
            }

           // ViewBag.Supplier = suppliers;
            ViewBag.partList = partList;

            return View(whrsIncome);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PART_WRHS_INCOMES whrsIncome)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db1 = new DBTHSNEntities())
                {
                    PART_WRHS_INCOMES wrhsIncomeToUpdate = db1.PART_WRHS_INCOMES.Find(whrsIncome.ID);
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
                        if (TryUpdateModel(wrhsIncomeToUpdate, "", new string[] { "ContractNo", "IssuedDate", "SupplierID", "Price", "Currency", "Amount", "Incoterms", "PaymentTerms", "DueDate", "IDN" }))
                        {
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
                    }
                    return View(wrhsIncomeToUpdate);
                }
            }
            return View(whrsIncome);
        }
        //hali toliq ozgartitilmagan
        /*
        public ActionResult EditPart(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contractPart = db.PART_WRHS_INCOME_PARTS
                                    .Include(p => p.P_CONTRACTS)
                                    .Include(p => p.PART)
                                    .FirstOrDefault(p => p.ID == ID);
                if (contractPart == null)
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

                return View(contractPart);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPart(PART_WRHS_INCOME_PARTS contractPart)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PART_WRHS_INCOME_PARTS contractPartToUpdate = db.PART_WRHS_INCOME_PARTS.Find(contractPart.ID);
                    if (contractPartToUpdate != null)
                    {
                        contractPartToUpdate.PartID = contractPart.PartID;
                        contractPartToUpdate.Price = contractPart.Price;
                        contractPartToUpdate.Quantity = contractPart.Quantity;
                        contractPartToUpdate.Unit = contractPart.Unit;
                        contractPartToUpdate.MOQ = contractPart.MOQ;
                        contractPartToUpdate.ActivePart = contractPart.ActivePart;
                        //contractPartToUpdate.Amount = contractPart.Quantity * contractPart.Price; SQL o'zi chiqarib beradi


                        if (TryUpdateModel(contractPartToUpdate, "", new string[] { "PartID, Price, Quantity, Unit, MOQ, ActivePart" }))
                        {
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
                    }
                    return View(contractPartToUpdate);
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
                var whrsIncome = db.P_CONTRACTS.Find(Id);
                if (whrsIncome == null)
                {
                    return HttpNotFound();
                }
                else
                    ViewBag.partList = db.PART_WRHS_INCOME_PARTS
                        .Include(pc => pc.PART)
                        .Where(pc => pc.ContractID == whrsIncome.ID).ToList();

                db.Entry(whrsIncome).Reference(i => i.SUPPLIER).Load();
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
                    P_CONTRACTS contractToDelete = db.P_CONTRACTS.Find(ID);
                    if (contractToDelete != null)
                    {
                        contractToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(contractToDelete).State = System.Data.Entity.EntityState.Modified;
                            /*var contractParts = db.PART_WRHS_INCOME_PARTS.Where(pc => pc.ContractID == contractToDelete.ID).ToList();
                            foreach (var contractPart in contractParts)
                            {
                                db.PART_WRHS_INCOME_PARTS.Remove(contractPart);
                            }
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
