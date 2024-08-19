using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
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
            return View();
        }

        public ActionResult Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES sampleFile = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("sql.xlsx") == 0).FirstOrDefault();
                if (sampleFile != null)
                    return File(sampleFile.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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
                                string productNo = row["Product No."].ToString();

                                SPL existingRecord = db.SPL.Where(s => s.ProdID.CompareTo(productNo) == 0 && s.IsDeleted == false).FirstOrDefault();
                                if (existingRecord != null)
                                {
                                    ViewBag.ExistingRecordsCount = 1;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = $"Error occurred while uploading the file: {ex.Message}";
                        return View("UploadWithExcel");
                    }
                }
                else
                {
                    ViewBag.Message = "Invalid format. Only .xlsx files are allowed.";
                    return View("UploadWithExcel");
                }
            }
            else
            {
                ViewBag.Message = "File is empty or not uploaded!";
                return View("UploadWithExcel");
            }
            return View("UploadWithExcel");
        }

        public ActionResult ClearDataTable()
        {
            ViewBag.DataTable = null;
            ViewBag.DataTableModel = null;
            ViewBag.IsFileUploaded = false;
            ViewBag.Message = "Table data has been cleared.";
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
                            SPL existingRecord = db.SPL.Where(s => s.ProdID.CompareTo(productNo) == 0 && s.IsDeleted == false).FirstOrDefault();
                            var prodID = db.PRODUCTS.Where(x => x.IsDeleted == false && x.PNo.CompareTo(productNo) == 0).FirstOrDefault().ID;
                            if (existingRecord == null)
                            {
                                SPL newSplRecord = new SPL();
                                newSplRecord.ProdID = prodID;
                                newSplRecord.CarModel1 = row["Model"].ToString();
                                newSplRecord.Option1 = row["OptionID"].ToString();
                                newSplRecord.Option1UsageQty = Convert.ToInt32(row["Usage"]);
                                newSplRecord.Option1UsageUnit = row["Unit"].ToString();
                                newSplRecord.IsActive = true;  // Assuming new entries are active by default
                                newSplRecord.IsDeleted = false; // Assuming new entries are not deleted by default

                                db.SPL.Add(newSplRecord);
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