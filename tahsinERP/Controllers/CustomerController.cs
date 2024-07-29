using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class CustomerController : Controller
    {
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
        private string supplierName = "";
        // GET: Customer
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var customers = db.CUSTOMERS.Where(x => x.IsDeleted == false).ToList();
                return View(customers);
            }
        }


        public ActionResult Details(int? Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var customer = db.CUSTOMERS.Find(Id);
                if (customer == null)
                {
                    return HttpNotFound();
                }
                return View(customer);
            }
        }


        public ActionResult Create()
        {
            ViewBag.Customer = ConfigurationManager.AppSettings["Customer"]?.Split(',').ToList() ?? new List<string>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name, Address,DUNS, Type, Country, City, Address, Telephone, Email, ContactPersonName, DirectorName, IsDeleted")] CUSTOMER customer)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        customer.IsDeleted = false;
                        db.CUSTOMERS.Add(customer);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, ex);
                }
                ViewBag.Customer = ConfigurationManager.AppSettings["Customer"]?.Split(',').ToList() ?? new List<string>();
                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "CustomerController", "Create[Post]");
                return View(customer);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                CUSTOMER customer = db.CUSTOMERS.Find(id);
                if (customer == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Customer = ConfigurationManager.AppSettings["Customer"]?.Split(',').ToList() ?? new List<string>();
                return View(customer);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID, Name, Address, DUNS, Type, Country, City, Telephone, Email, ContactPersonName, DirectorName, IsDeleted")] CUSTOMER customer)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    customer.IsDeleted = false;
                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Customer = ConfigurationManager.AppSettings["Customer"]?.Split(',').ToList() ?? new List<string>();
            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "CustomerController", "Edit[Post]");
            return View(customer);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                CUSTOMER customer = db.CUSTOMERS.Find(id);
                if (customer == null)
                {
                    return HttpNotFound();
                }

                return View(customer);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                CUSTOMER customer = db.CUSTOMERS.Find(id);
                if (customer != null)
                {
                    db.CUSTOMERS.Remove(customer);
                    db.SaveChanges();
                }

                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "CustomerController", "Delete[Post]");
                return RedirectToAction("Index");
            }
        }


        public ActionResult Download()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
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

                        using(DBTHSNEntities db = new DBTHSNEntities())
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
            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "CustomerController", "ClearDataTable");
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
                    using(DBTHSNEntities db = new DBTHSNEntities())
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
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "CustomerController", "Save[Post]");
            return RedirectToAction("Index");
        }
    }
}