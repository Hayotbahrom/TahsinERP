using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class SPLController : Controller
    {
        // GET: SPL
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = db.SPLs.Where(x => x.IsDeleted == false).ToList();
                return View(list);
            }
        }

        public ActionResult Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES sampleFile = db.SAMPLE_FILES.FirstOrDefault(s => s.FileName.CompareTo("spl.xlsx") == 0);
                if (sampleFile != null)
                    return File(sampleFile.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                return View();
            }
        }

        public ActionResult UploadWithExcel()
        {
            ViewBag.IsFileUploaded = false;
            ViewBag.ExistingRecordsCount = 0;
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

                        int duplicateCount = 0;
                        using (DBTHSNEntities db = new DBTHSNEntities())
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                string productNo = row["Product No."].ToString();

                                SPL existingRecord = db.SPLs.FirstOrDefault(s => s.ProdID.CompareTo(productNo) == 0 && s.IsDeleted == false);
                                if (existingRecord != null)
                                {
                                    duplicateCount++;
                                }
                            }
                        }
                        ViewBag.ExistingRecordsCount = duplicateCount;

                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = $"Faylni yuklashda xatolik: {ex.Message}";
                        return View("UploadWithExcel");
                    }
                }
                else
                {
                    ViewBag.Message = "Noto'g'ri format. Faqat .xlsx fayllar qabul qilinadi.";
                    return View("UploadWithExcel");
                }
            }
            else
            {
                ViewBag.Message = "Fayl tanlanmagan yoki fayl bo'sh!";
                return View("UploadWithExcel");
            }
            return View("UploadWithExcel");
        }

        public ActionResult ClearDataTable()
        {
            ViewBag.DataTable = null;
            ViewBag.DataTableModel = null;
            ViewBag.IsFileUploaded = false;
            ViewBag.Message = "Jadval ma'lumotlari tozalandi.";
            return View("UploadWithExcel");
        }

        [HttpPost]
        public ActionResult Save(string dataTableModel)
        {
            if (!string.IsNullOrEmpty(dataTableModel))
            {
                var tableModel = JsonConvert.DeserializeObject<DataTable>(dataTableModel);

                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        foreach (DataRow row in tableModel.Rows)
                        {
                            string productNo = row["Product No."].ToString();
                            var existingRecord = new SPL();
                            if (db.SPLs.Where(x => x.IsDeleted == false).ToList().Count == 0)
                            {
                                existingRecord = null;
                            }
                            else
                                existingRecord = db.SPLs.FirstOrDefault(s => s.ProdID.CompareTo(productNo) == 0 && s.IsDeleted == false);
                            
                            var prodID = db.PRODUCTS.FirstOrDefault(x => x.IsDeleted == false && x.PNo.CompareTo(productNo) == 0)?.ID;
                            if (prodID is null)
                            {
                                PRODUCT newProduct = new PRODUCT
                                {
                                    PNo = productNo,
                                    Name = row["Name"].ToString()
                                };
                                db.PRODUCTS.Add(newProduct);
                                db.SaveChanges();
                                prodID = db.PRODUCTS.Where(x => x.PNo.CompareTo(newProduct.PNo)==0 && x.Name.CompareTo(newProduct.Name)==0).FirstOrDefault().ID;
                            }
                            if (existingRecord == null && prodID != null)
                            {
                                SPL newSplRecord = new SPL
                                {
                                    ProdID = (int)prodID,
                                    CarModel1 = row["Model"].ToString(),
                                    Option1 = row["OptionID"].ToString(),
                                    Option1UsageQty = Convert.ToInt32(row["Usage"]),
                                    Option1UsageUnit = row["Unit"].ToString(),
                                    IsActive = true,  // Assuming new entries are active by default
                                    IsDeleted = false // Assuming new entries are not deleted by default
                                };

                                db.SPLs.Add(newSplRecord);
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
            LogHelper.LogToDatabase(userEmail, "SPLController", "Save[Post]");
            return RedirectToAction("Index");
        }
    }
}
