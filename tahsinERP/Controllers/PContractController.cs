using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls.WebParts;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class PContractController : Controller
    {
        private string supplierName, contractNo, partNo = "";
        private int contractDocMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        private List<string> missingSuppliers = new List<string>();
        private List<string> missingParts = new List<string>();

        private string[] sources;
        public PContractController()
        {
            sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
            sources = sources.Where(x => !x.Equals("InHouse", StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        // GET: Contracts
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var suppliers = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
                if (!string.IsNullOrEmpty(type))
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.P_CONTRACTS.Include(x => x.SUPPLIER).Where(s => s.SupplierID == supplierID && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.IsDeleted == false && x.Type.CompareTo(type) == 0), "ID", "Name");
                    }
                    else
                    {
                        ViewBag.partList = db.P_CONTRACTS.Include(x => x.SUPPLIER).Where(s => s.IsDeleted == false && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.IsDeleted == false && x.Type.CompareTo(type) == 0), "ID", "Name");
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.P_CONTRACTS.Include(x => x.SUPPLIER).Where(s => s.IsDeleted == false && s.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.IsDeleted == false), "ID", "Name");
                    }
                    else
                    {
                        ViewBag.partList = db.P_CONTRACTS.Include(x => x.SUPPLIER).Where(s => s.IsDeleted == false).ToList();
                        ViewBag.SourceList = new SelectList(sources);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.IsDeleted == false), "ID", "Name");
                    }
                }
                return View();
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
                        if (CheckForExistenceOfSuppliers(dataTable))
                        {
                            if (CheckForExistenceOfParts(dataTable))
                            {
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
                                            if (part != null)
                                            {
                                                P_CONTRACT_PARTS contractPart = db.P_CONTRACT_PARTS.Where(pcp => pcp.PartID == part.ID && pcp.ContractID == contract.ID).FirstOrDefault();
                                                if (contractPart != null)
                                                    ViewBag.ExistingRecordsCount = 1;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var message = "";
                                foreach (var word in missingParts)
                                {
                                    message += word + ", ";
                                }
                                ViewBag.Message = "Ushbu shartnomalar faylida kiritilgan ehtiyot qismlar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Ehtiyot qismlar bazasiga kiritib keyin qayta urining.";
                                return View("UploadWithExcel");
                            }
                        }
                        else
                        {
                            var message = "";
                            foreach (var word in missingSuppliers)
                            {
                                message += word + ", ";
                            }
                            ViewBag.Message = "Ushbu shartnomalar faylda kiritilgan ta'minotchilar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Ta'minotchilar bazasiga kiritib keyin qayta urining.";
                            return View("UploadWithExcel");
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
        private bool CheckForExistenceOfParts(DataTable dataTable)
        {
            bool flag = false;
            if (dataTable != null)
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string partPNo = row["Part Number"].ToString();
                        if (db.PARTS.Where(p => p.PNo.CompareTo(partPNo) == 0).Any())
                            flag = true;
                        else
                        {
                            if (!missingParts.Contains(partPNo))
                                missingParts.Add(partPNo);
                        }
                    }
                    if (missingParts.Count > 0)
                        return false;
                    else
                        return flag;
                }
            else
                return flag;
        }
        private bool CheckForExistenceOfSuppliers(DataTable dataTable)
        {
            bool flag = false;
            if (dataTable != null)
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string supplierName = row["Supplier Name"].ToString();
                        if (db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0).Any())
                            flag = true;
                        else
                        {
                            if (!missingSuppliers.Contains(supplierName))
                                missingSuppliers.Add(supplierName);
                        }
                    }
                    if (missingSuppliers.Count > 0)
                        return false;
                    else
                        return flag;
                }
            else
                return flag;

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
                                    UNIT unit = db.UNITS.Where(u => u.ShortName.CompareTo(row["Unit"].ToString()) == 0).FirstOrDefault();
                                    if (unit != null)
                                        new_contractPart.UNIT = unit;
                                    else
                                        new_contractPart.UnitID = 1;
                                    new_contractPart.MOQ = Convert.ToDouble(row["MOQ"].ToString());
                                    new_contractPart.Quantity = Convert.ToDouble(row["Amount"].ToString());
                                    new_contractPart.ActivePart = true;

                                    db.P_CONTRACT_PARTS.Add(new_contractPart);
                                    //int noOfRowUpdated = db.Database.ExecuteSqlCommand("UPDATE P_CONTRACT_PARTS SET ActivePart =" + 0 + " WHERE ContractID !=" + new_contract.ID + " AND PartID =" + part.ID + "");

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
                                    UNIT unit = db.UNITS.Where(u => u.ShortName.CompareTo(row["Unit"].ToString()) == 0).FirstOrDefault();
                                    if (unit != null)
                                        new_contractPart.UNIT = unit;
                                    else
                                        new_contractPart.UnitID = 1;
                                    new_contractPart.MOQ = Convert.ToDouble(row["MOQ"].ToString());
                                    new_contractPart.Quantity = Convert.ToDouble(row["Amount"].ToString());
                                    new_contractPart.ActivePart = true;

                                    db.P_CONTRACT_PARTS.Add(new_contractPart);
                                    int noOfRowUpdated = db.Database.ExecuteSqlCommand("UPDATE P_CONTRACT_PARTS SET ActivePart =" + 0 + " WHERE ContractID !=" + contract.ID + " AND PartID =" + part.ID + "");

                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "POrderController", "Save[Post]");
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
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.partList = new SelectList(db.PARTS.Where(c => c.IsDeleted == false).ToList(), "ID", "PNo");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PContractViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    var newContract = new P_CONTRACTS()
                    {
                        ContractNo = model.ContractNo,
                        IssuedDate = model.IssuedDate,
                        CompanyID = 1,
                        SupplierID = model.SupplierID,
                        Currency = model.Currency,
                        Amount = model.Amount,
                        Incoterms = model.Incoterms,
                        PaymentTerms = model.PaymentTerms,
                        DueDate = model.DueDate,
                        IsDeleted = false,
                        IDN = model.IDN,
                    };
                    db.P_CONTRACTS.Add(newContract);
                    db.SaveChanges();

                    // Yangi yozuvning IncomeID sini olish
                    int newContractID = newContract.ID;

                    // Parts ni saqlash
                    foreach (var part in model.Parts)
                    {
                        var newPart = new P_CONTRACT_PARTS
                        {
                            ContractID = newContractID, // part.IncomeID emas, yangi yaratilgan IncomeID ishlatiladi
                            PartID = part.PartID,
                            UnitID = part.UnitID,
                            Price = part.Price,
                            Quantity = part.Quantity,
                            ActivePart = true,
                            MOQ = part.MOQ
                        };

                        db.P_CONTRACT_PARTS.Add(newPart);
                    }

                    db.SaveChanges();

                    if (Request.Files["docUpload"].ContentLength > 0)
                    {
                        if (Request.Files["docUpload"].InputStream.Length < 5242880)
                        {
                            P_CONTRACT_DOCS contractDoc = new P_CONTRACT_DOCS();
                            byte[] avatar = new byte[Request.Files["partPhotoUpload"].InputStream.Length];
                            Request.Files["partPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
                            contractDoc.ContractID = newContract.ID;
                            contractDoc.Doc = avatar;

                            db.P_CONTRACT_DOCS.Add(contractDoc);
                            db.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("", "Faylni yuklab bo'lmadi, u 2MBdan kattaroq. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
                            throw new RetryLimitExceededException();
                        }
                    }
                }
                catch
                {
                    if (!ModelState.IsValid && Request.Files["partPhotoUpload"].ContentLength <= 0)
                    {
                        ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                        ViewBag.partList = new SelectList(db.PARTS.Where(c => c.IsDeleted == false).ToList(), "ID", "PNo");
                        ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                        return View(model);
                    }
                }


                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "PContractController", "Create[Post]");
                return RedirectToAction("Index");
            }
        }
        public ActionResult DownloadDoc(int? contractID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contractDoc = db.P_CONTRACT_DOCS.FirstOrDefault(pi => pi.ContractID == contractID);
                if (contractDoc != null)
                    return File(contractDoc.Doc, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                return HttpNotFound("Fayl yuklanmagan");
            }
        }
        public ActionResult Details(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contract = db.P_CONTRACTS
                                  .Include(p => p.SUPPLIER)
                                  .FirstOrDefault(p => p.ID == ID && p.IsDeleted == false);

                if (contract == null)
                {
                    return HttpNotFound();
                }

                var partList = db.P_CONTRACT_PARTS
                                  .Include(pc => pc.PART)
                                  .Include(pc => pc.UNIT)
                                  .Where(pc => pc.ContractID == contract.ID)
                                  .ToList();

                ViewBag.PartList = partList;
                var partImage = db.P_CONTRACT_DOCS.FirstOrDefault(pi => pi.ContractID == ID);
                if (partImage != null)
                {
                    ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String(partImage.Doc);
                }
                return View(contract);
            }
        }
        public ActionResult GetPartsBySupplier(int supplierId)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var supplier = db.SUPPLIERS.Where(x => x.IsDeleted == false && x.ID == supplierId).FirstOrDefault();
                var parts = db.PARTS
                    .Where(p => p.Type.CompareTo(supplier.Type) == 0 && p.IsDeleted == false)
                    .Select(p => new { p.ID, p.PNo })
                    .ToList();

                return Json(parts, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            P_CONTRACTS contract;
            List<P_CONTRACT_PARTS> partList;
            SelectList suppliers;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                contract = db.P_CONTRACTS
                    .Include(c => c.SUPPLIER)
                    .Include(c => c.P_CONTRACT_PARTS.Select(p => p.PART))
                    .FirstOrDefault(c => c.ID == ID && c.IsDeleted == false);

                if (contract == null)
                {
                    return HttpNotFound();
                }

                suppliers = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", contract.SupplierID);
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                partList = contract.P_CONTRACT_PARTS.ToList();
            }

            ViewBag.Supplier = suppliers;
            ViewBag.partList = partList;
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(P_CONTRACTS contract)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
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
                        contractToUpdate.IDN = contract.IDN;
                        contractToUpdate.IsDeleted = false;

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
                    return View(contractToUpdate);
                }
            }
            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "PContractController", "Edit[Post]");
            return View(contract);
        }
        public ActionResult EditPart(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contractPart = db.P_CONTRACT_PARTS
                                    .Include(p => p.P_CONTRACTS)
                                    .Include(p => p.PART)
                                    .FirstOrDefault(p => p.ID == ID);
                if (contractPart == null)
                {
                    return HttpNotFound();
                }
                var allParts = db.PARTS
                                .Include(p => p.P_CONTRACT_PARTS)
                                .Include(p => p.UNIT)
                                .Where(p => p.IsDeleted == false)
                                .ToList();

                ViewBag.PartList = new SelectList(allParts, "ID", "PNo");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

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
                        contractPartToUpdate.UnitID = contractPart.UnitID;
                        contractPartToUpdate.MOQ = contractPart.MOQ;
                        contractPartToUpdate.ActivePart = contractPart.ActivePart;
                        //contractPartToUpdate.Amount = contractPart.Quantity * contractPart.Price; SQL o'zi chiqarib beradi
                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PContractController", "EditPart[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }

                    }
                    return RedirectToAction("Edit");
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
                    ViewBag.partList = db.P_CONTRACT_PARTS
                        .Include(pc => pc.PART)
                        .Include(pc => pc.UNIT)
                        .Where(pc => pc.ContractID == contract.ID).ToList();

                db.Entry(contract).Reference(i => i.SUPPLIER).Load();
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
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PContractController", "Delete[Post]");
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
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PContractController", "DeletePart[Post]");
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