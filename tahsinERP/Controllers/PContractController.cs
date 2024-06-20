using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PContractController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private string[] sources = ConfigurationManager.AppSettings["p_contractTypes"].Split(',');
        private string supplierName, contractNo, partNo = "";
        // GET: Contracts
        public ActionResult Index(string type)
        {
           
                if (!string.IsNullOrEmpty(type))
                {
                    List<P_CONTRACTS> list = db.P_CONTRACTS.Where(pc => pc.SUPPLIER.Type.CompareTo(type) == 0 && pc.IsDeleted == false).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    return View(list);
                }
                else
                {
                    List<P_CONTRACTS> list = db.P_CONTRACTS.Where(pc => pc.IsDeleted == false).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    return View(list);
                }
           
        }
        public ActionResult Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES shartnoma = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("shartnoma.xlsx") == 0).FirstOrDefault();
                if (shartnoma != null)
                    return File(shartnoma.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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
            if (file != null && file.ContentLength > 0)
            {
                if (Path.GetExtension(file.FileName).ToLower() == ".xlsx")
                {
                    try
                    {
                        var dataTable = new System.Data.DataTable();
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

                        using (DBTHSNEntities db = new DBTHSNEntities())
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                contractNo = row["ContractNo"].ToString();
                                supplierName = row["Supplier Name"].ToString();
                                partNo = row["Part Number"].ToString();

                                SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                                PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                P_CONTRACTS contract = db.P_CONTRACTS.Where(pc => pc.ContractNo.CompareTo(contractNo) == 0 && pc.SupplierID == supplier.ID && pc.IsDeleted == false).FirstOrDefault();
                                if (contract != null)
                                {
                                    P_CONTRACT_PARTS contractPart = db.P_CONTRACT_PARTS.Where(pcp => pcp.PartID == part.ID && pcp.ContractID == contract.ID).FirstOrDefault();
                                    if (contractPart != null)
                                        ViewBag.ExistingRecordsCount = 1;
                                }
                            }
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
                var tableModel = JsonConvert.DeserializeObject<System.Data.DataTable>(dataTableModel);

                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        foreach (DataRow row in tableModel.Rows)
                        {
                            contractNo = row["ContractNo"].ToString();
                            supplierName = row["Supplier Name"].ToString();
                            partNo = row["Part Number"].ToString();

                            SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();

                            P_CONTRACTS contract = db.P_CONTRACTS.Where(pc => pc.ContractNo.CompareTo(contractNo) == 0 && pc.IsDeleted == false).FirstOrDefault();

                            //
                            if (contract == null)
                            {
                                P_CONTRACTS new_contract = new P_CONTRACTS();
                                new_contract.ContractNo = contractNo;
                                new_contract.SupplierID = supplier.ID;
                                new_contract.IssuedDate = DateTime.Parse(row["IssuedDate"].ToString());
                                new_contract.DueDate = DateTime.Parse(row["DueDate"].ToString());
                                new_contract.Incoterms = row["Incoterms"].ToString();
                                new_contract.PaymentTerms = row["PaymentTerms"].ToString();
                                new_contract.Currency = row["Currency"].ToString();
                                new_contract.CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["companyID"]);
                                new_contract.IsDeleted = false;

                                db.P_CONTRACTS.Add(new_contract);
                                db.SaveChanges();

                                P_CONTRACT_PARTS contractPart = db.P_CONTRACT_PARTS.Where(pcp => pcp.ContractID == new_contract.ID && pcp.PartID == part.ID).FirstOrDefault();
                                if (contractPart == null)
                                {
                                    P_CONTRACT_PARTS new_contractPart = new P_CONTRACT_PARTS();
                                    new_contractPart.PartID = part.ID;
                                    new_contractPart.ContractID = new_contract.ID;
                                    new_contractPart.Price = Convert.ToDouble(row["Price"].ToString());
                                    new_contractPart.Unit = row["Unit"].ToString();
                                    new_contractPart.MOQ = Convert.ToDouble(row["MOQ"].ToString());
                                    new_contractPart.Quantity = Convert.ToDouble(row["Amount"].ToString());
                                    new_contractPart.ActivePart = true;

                                    db.P_CONTRACT_PARTS.Add(new_contractPart);
                                    int noOfRowUpdated = db.Database.ExecuteSqlCommand("UPDATE P_CONTRACT_PARTS SET ActivePart =" + 0 + " WHERE ContractID !=" + new_contract.ID + " AND PartID =" + part.ID + "");

                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                P_CONTRACT_PARTS contractPart = db.P_CONTRACT_PARTS.Where(pcp => pcp.ContractID == contract.ID && pcp.PartID == part.ID).FirstOrDefault();
                                if (contractPart == null)
                                {
                                    P_CONTRACT_PARTS new_contractPart = new P_CONTRACT_PARTS();
                                    new_contractPart.PartID = part.ID;
                                    new_contractPart.ContractID = contract.ID;
                                    new_contractPart.Price = Convert.ToDouble(row["Price"].ToString());
                                    new_contractPart.Unit = row["Unit"].ToString();
                                    new_contractPart.MOQ = Convert.ToDouble(row["MOQ"].ToString());
                                    new_contractPart.Quantity = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_CONTRACT_PARTS.Add(new_contractPart);
                                    int noOfRowUpdated = db.Database.ExecuteSqlCommand("UPDATE P_CONTRACT_PARTS SET ActivePart =" + 0 + " WHERE ContractID !=" + contract.ID + " AND PartID =" + part.ID + "");

                                    db.SaveChanges();
                                }
                            }
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
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name");
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContractNo, IssuedDate, CompanyID, SupplierID, PartID, Price, Currency, Amount, Incoterms, PaymentTerms, MOQ,MaximumCapacity, Unit,DueDate")] P_CONTRACTS contract)
        {

            try
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    if (ModelState.IsValid)
                    {
                        db.P_CONTRACTS.Add(contract);
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
                var contract = db.P_CONTRACTS.Find(ID);
                if (contract == null)
                {
                    return HttpNotFound();
                }
                else
                    ViewBag.partList = db.P_CONTRACT_PARTS.Where(pc => pc.ContractID == contract.ID).ToList();
                return View(contract);
        }
        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            /*using (DBTHSNEntities db = new DBTHSNEntities())
            {*/
                var contract = db.P_CONTRACTS.Find(ID);
                if (contract == null)
                {
                    return HttpNotFound();
                }
                
                ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name", contract.SupplierID);
                ViewBag.partList = db.P_CONTRACT_PARTS.Where(pc => pc.ContractID == contract.ID).ToList();

                db.Entry(contract).Reference(c => c.SUPPLIER).Load();
                return View(contract);
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(P_CONTRACTS contract)
        {
            if (ModelState.IsValid)
            {
               /* using (DBTHSNEntities db = new DBTHSNEntities())
                {*/
                    P_CONTRACTS contractToUpdate = db.P_CONTRACTS.Find(contract.ID);
                    if (contractToUpdate != null)
                    {
                        contractToUpdate.ContractNo = contract.ContractNo;
                        contractToUpdate.SupplierID = contract.SupplierID;
                        contractToUpdate.IssuedDate = contract.IssuedDate;
                        contractToUpdate.DueDate = contract.DueDate;
                        contractToUpdate.Currency = contract.Currency;
                        contractToUpdate.Incoterms = contract.Incoterms;
                        contractToUpdate.PaymentTerms = contract.PaymentTerms;
                        contractToUpdate.IsDeleted = false;

                        if (TryUpdateModel(contractToUpdate, "", new string[] { "ContractNo, IssuedDate, SupplierID, Price, Currency, Amount, Incoterms, PaymentTerms, DueDate" }))
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
                    return View(contractToUpdate);
                
            }
            return View();
        }
        public ActionResult EditPart(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contractPart = db.P_CONTRACT_PARTS.Find(ID);
                if (contractPart == null)
                {
                    return HttpNotFound();
                }
                var allParts = db.PARTS.Select(p => new SelectListItem
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
        public ActionResult EditPart(P_CONTRACT_PARTS contractPart)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    P_CONTRACT_PARTS contractPartToUpdate = db.P_CONTRACT_PARTS.Find(contractPart.ID);
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
                var contract = db.P_CONTRACTS.Find(Id);
                if (contract == null)
                {
                    return HttpNotFound();
                }
                else
                    ViewBag.partList = db.P_CONTRACT_PARTS.Where(pc => pc.ContractID == contract.ID).ToList();
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
                    P_CONTRACTS contractToDelete = db.P_CONTRACTS.Find(ID);
                    if (contractToDelete != null)
                    {
                        contractToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(contractToDelete).State = System.Data.Entity.EntityState.Modified;
                            var contractParts = db.P_CONTRACT_PARTS.Where(pc => pc.ContractID == contractToDelete.ID).ToList();
                            foreach (var contractPart in contractParts)
                            {
                                db.P_CONTRACT_PARTS.Remove(contractPart);
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
                P_CONTRACT_PARTS contractPartToDelete = db.P_CONTRACT_PARTS.Find(id);
                if (ModelState.IsValid)
                {
                    if (contractPartToDelete != null)
                    {
                        try
                        {
                            db.P_CONTRACT_PARTS.Remove(contractPartToDelete);
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
        }
    }
}