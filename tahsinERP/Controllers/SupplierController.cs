using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class SupplierController : Controller
    {
        private string[] sources;
        private string supplierName = "";
        public SupplierController()
        {
            sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
            sources = sources.Where(x => !x.Equals("InHouse", StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        // GET: Supplier
        public ActionResult Index(string type)
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                if (!string.IsNullOrEmpty(type))
                {
                    List<SUPPLIER> list = db.SUPPLIERS.Where(s => s.IsDeleted == false && s.Type.CompareTo(type) == 0).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.Type = type;

                    return View(list);
                }
                else
                {
                    List<SUPPLIER> list = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.Type = type;

                    return View(list);
                }
            }
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(/*[Bind(Include = "Name, DUNS, Type, Country, City, Address, Telephone, E_mail, ContactPerson, Director, IsDeleted")]*/ SUPPLIER supplier)
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var existSupplier = db.SUPPLIERS.Where(x => x.IsDeleted == false && x.Name.ToLower().CompareTo(supplier.Name.ToLower()) == 0).FirstOrDefault();
                        if (existSupplier is null)
                        {
                            supplier.IsDeleted = false;
                            db.SUPPLIERS.Add(supplier);
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "SupplierController", $"{supplier.Name} - Supplierni yaratdi");

                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Bunday nom bilan Ta'minotchi kiritilgan, ma'lumotlarni qaytadan tekshiring.");
                            return View(supplier);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, ex);
                }
                return View(supplier);
            }
        }
        public ActionResult Details(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var supplier = db.SUPPLIERS.Find(Id);
                if (supplier == null)
                {
                    return HttpNotFound();
                }
                ViewBag.partList = SupplierParts(supplier.ID);
                return View(supplier);
            }
        }
        public List<GetSupplierParts_Result> SupplierParts(int? supplierId)
        {
            if (supplierId == null)
            {
                // Handle missing SupplierID (e.g., show an error message)
                return null;
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var supplierData = db.Database.SqlQuery<GetSupplierParts_Result>(
                    "EXEC GetSupplierParts @SupplierID",
                    new SqlParameter("@SupplierID", supplierId)
                ).ToList();

                return supplierData;
            }
        }
        public ActionResult Edit(int? Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var supplier = db.SUPPLIERS.Find(Id);
                if (supplier == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Type = new SelectList(sources, supplier.Type);
                return View(supplier);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SUPPLIER supplier)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    SUPPLIER supplierToUpdate = db.SUPPLIERS.Find(supplier.ID);
                    if (supplierToUpdate != null)
                    {
                        supplierToUpdate.IsDeleted = false;
                        supplierToUpdate.Type = supplier.Type;
                        if (TryUpdateModel(supplierToUpdate, "", new string[] { "Name", "DUNS", "Type", "Country", "City", "Address", "Telephone", "Email", "ContactPersonName", "DirectorName", "IsDeleted" }))
                        {
                            try
                            {
                                var existSupplier = db.SUPPLIERS.Where(x => x.IsDeleted == false && x.Name.ToLower().CompareTo(supplier.Name.ToLower()) == 0 && x.ID != supplierToUpdate.ID).FirstOrDefault();
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "SupplierController", $"{supplierToUpdate.Name} - Supplierni tahrirladi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(supplierToUpdate);
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
                var supplier = db.SUPPLIERS.Find(Id);
                if (supplier == null)
                {
                    return HttpNotFound();
                }

                return View(supplier);
            }
            //return RedirectToAction("SupplierParts?supplierId="+supplier.ID);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    SUPPLIER supplierToDelete = db.SUPPLIERS.Find(ID);
                    if (supplierToDelete != null)
                    {
                        supplierToDelete.IsDeleted = true;
                        if (TryUpdateModel(supplierToDelete, "", new string[] { "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "SupplierController", $"{supplierToDelete.Name} - Supplierni o'chirdi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                }
            }
            return View();
        }
        public ActionResult Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES taminotchilar = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("taminotchilar.xlsx") == 0).FirstOrDefault();
                if (taminotchilar != null)
                    return File(taminotchilar.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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

                        using (DBTHSNEntities db = new DBTHSNEntities())
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                supplierName = row["Name"].ToString();

                                SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                                if (supplier != null)
                                    ViewBag.ExistingRecordsCount = 1;
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
                var tableModel = JsonConvert.DeserializeObject<DataTable>(dataTableModel);
                // Save to the database
                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        foreach (DataRow row in tableModel.Rows)
                        {
                            supplierName = row["Name"].ToString();
                            SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();

                            if (supplier == null)
                            {
                                SUPPLIER new_supplier = new SUPPLIER();
                                new_supplier.Name = supplierName;
                                new_supplier.Address = row["Address"].ToString();
                                new_supplier.Country = row["Country"].ToString();
                                new_supplier.City = row["City"].ToString();
                                new_supplier.Type = row["Type"].ToString();
                                new_supplier.DUNS = row["DUNS"].ToString();
                                new_supplier.Email = row["Email"].ToString();
                                new_supplier.Telephone = row["Telephone"].ToString();
                                new_supplier.ContactPersonName = row["ContactPersonName"].ToString();
                                new_supplier.DirectorName = row["DirectorName"].ToString();
                                new_supplier.IsDeleted = false;

                                db.SUPPLIERS.Add(new_supplier);
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "SupplierController", $"{new_supplier.Name} - Supplierni Excell orqali yaratdi");
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
    }
}