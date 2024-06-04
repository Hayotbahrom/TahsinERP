using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using System.Web.WebPages;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PContractController : Controller
    {

        private DBTHSNEntities db = new DBTHSNEntities();
        private string[] sources = new string[3] { "", "Import", "Lokal" };
        string supplierName, contractNo, partNo = "";


        // GET: Contracts
        public ActionResult Index(string type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                List<P_CONTRACTS> list = db.P_CONTRACTS.Join(db.SUPPLIERS, c => c.SupplierID, s => s.ID, (c, s) => new { Contract = c, Supplier = s }).
                    Where(cs => cs.Supplier.Type.CompareTo(type) == 0).
                    Select(cs => cs.Contract).ToList();
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
            ViewBag.Supplier = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name");
            return View();
        }
        public ActionResult Download()
        {
            SAMPLE_FILES shartnoma = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("shartnoma.xlsx") == 0).FirstOrDefault();
            if (shartnoma != null)
                return File(shartnoma.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return View();
        }
        public ActionResult UploadWithExcel()
        {
            ViewBag.IsFileUploaded = false;
            return View();
        }
        [HttpPost]
        public ActionResult UploadWithExcel(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                if (Path.GetExtension(file.FileName).ToLower() == ".xlsx")
                {
                    try
                    {
                        var dataTable = new DataTable();
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;
                            var colCount = worksheet.Dimension.Columns;

                            for (int col = 1; col <= colCount; col++)
                            {
                                dataTable.Columns.Add(worksheet.Cells[1, col].Text);
                            }

                            for (int row = 2; row <= rowCount; row++)
                            {
                                var dataRow = dataTable.NewRow();
                                for (int col = 1; col <= colCount; col++)
                                {
                                    dataRow[col - 1] = worksheet.Cells[row, col].Text;
                                }
                                dataTable.Rows.Add(dataRow);
                            }
                        }

                        ViewBag.DataTable = dataTable;
                        ViewBag.DataTableModel = JsonConvert.SerializeObject(dataTable);
                        ViewBag.IsFileUploaded = true;


                        foreach (DataRow row in dataTable.Rows)
                        {
                            contractNo = row["ContractNo"].ToString();
                            supplierName = row["Supplier Name"].ToString();
                            partNo = row["Part Number"].ToString();

                            SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0).FirstOrDefault();
                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0).FirstOrDefault();
                            P_CONTRACTS checkContract = db.P_CONTRACTS.Where(pc => pc.ContractNo.CompareTo(contractNo) == 0 && pc.PART.PNo.CompareTo(partNo) == 0 && pc.SUPPLIER.Name.CompareTo(supplierName) == 0).FirstOrDefault();
                            if (checkContract != null)
                                ViewBag.ExistingRecordsCount = 1;

                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = $"Faylni yuklashda quyidagicha muammo tug'ildi: {ex.Message}";
                        return View("UploadWithExcel");
                    }
                }
                else
                {
                    ViewBag.Message = "Format noto'g'ri. Faqat .xlsx fayllarni yuklash mumkin.";
                    return View("UploadWithExcel");
                }
            }
            else
            {
                ViewBag.Message = "Fayl bo'm-bo'sh yoki yuklanmadi!";
                return View("UploadWithExcel");
            }
            return View("UploadWithExcel");
        }
        public ActionResult ClearDataTable()
        {
            // Clear the DataTable and related ViewBag properties
            ViewBag.DataTable = null;
            ViewBag.DataTableModel = null;
            ViewBag.IsFileUploaded = false;
            ViewBag.Message = "Jadval ma'lumotlari o'chirib yuborildi.";

            // Return the UploadWithExcel view
            return View("UploadWithExcel");
        }
        [HttpPost]
        public ActionResult Save(string dataTableModel)
        {

            if (!string.IsNullOrEmpty(dataTableModel))
            {
                var tableModel = JsonConvert.DeserializeObject<DataTable>(dataTableModel);

                // Save to the database
                try
                {
                    foreach (DataRow row in tableModel.Rows)
                    {
                        contractNo = row["ContractNo"].ToString();

                        SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0).FirstOrDefault();
                        PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0).FirstOrDefault();

                        P_CONTRACTS contract = new P_CONTRACTS();
                        if (supplier != null && part != null)
                        {
                            contract.ContractNo = contractNo;
                            contract.SupplierID = supplier.ID;
                            contract.IssuedDate = DateTime.Parse(row["IssuedDate"].ToString());
                            contract.DueDate = DateTime.Parse(row["DueDate"].ToString());
                            contract.Incoterms = row["Incoterms"].ToString();
                            contract.PaymentTerms = row["PaymentTerms"].ToString();
                            contract.PartID = part.ID;
                            contract.Price = Convert.ToDouble(row["Price"].ToString());
                            contract.Currency = row["Currency"].ToString();
                            contract.Amount = Convert.ToDouble(row["Amount"].ToString());
                            contract.Unit = row["Unit"].ToString();
                            contract.MOQ = Convert.ToDouble(row["MOQ"].ToString());
                            contract.CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["companyID"]);

                            db.P_CONTRACTS.Add(contract);
                            db.SaveChanges();
                        }

                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return RedirectToAction("Index");
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
            ViewBag.partList = db.P_CONTRACTS.Where(pc => pc.ContractNo.CompareTo(contract.ContractNo) == 0).ToList();
            return View(contract);
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
            ViewBag.partList = db.P_CONTRACTS.Where(pc => pc.ContractNo.CompareTo(contract.ContractNo) == 0).ToList();
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

        public ActionResult EditPart(int? Id)
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
            ViewBag.partList = db.P_CONTRACTS.Where(pc => pc.ContractNo.CompareTo(contract.ContractNo) == 0).ToList();
            return View(contract);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPart(P_CONTRACTS contract)
        {
            if (ModelState.IsValid)
            {
                P_CONTRACTS contractToUpdate = db.P_CONTRACTS.Find(contract.ID);
                if (contractToUpdate != null)
                {
                    contractToUpdate.PartID = contract.PartID;
                    contractToUpdate.Price = contract.Price;
                    contractToUpdate.Currency = contract.Currency;
                    contractToUpdate.Amount = contract.Amount;
                    contractToUpdate.MOQ = contract.MOQ;
                    contractToUpdate.MaximumCapacity = contract.MaximumCapacity;
                    contractToUpdate.Unit = contract.Unit;

                    if (TryUpdateModel(contractToUpdate, "", new string[] { "PartID", "Price", "Currency", "Amount", "MOQ", "MaximumCapacity", "Unit" }))
                    {
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (DbEntityValidationException ex)
                        {
                            foreach (var validationErrors in ex.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                                }
                            }
                            ModelState.AddModelError("", "Validation failed. Check the details and try again.");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                        }
                    }
                }
                return View(contractToUpdate);
            }
            return View(contract);
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