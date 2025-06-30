using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
                var list = db.SPLs
                    .Where(x => x.IsDeleted == false)
                    .Include(x => x.PRODUCT)
                    .ToList();
                return View(list);
            }
        }

        public async Task<ActionResult> Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.products = new SelectList(await db.PRODUCTS.Where(x => x.IsDeleted == false).ToListAsync(), "ID", "PNo");

                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SPL spl)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        // Set IsDeleted to false and save the spl to get the ID
                        spl.IsDeleted = false;

                        db.SPLs.Add(spl);
                        await db.SaveChangesAsync();

                        LogHelper.LogToDatabase(User.Identity.Name, "SPLController", $"{spl.PRODUCT.PNo} ga ega {spl.CarModel1} ni yaratdi");

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }

            }

            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "SPLController", "Create[Post]");
            return View(spl);
        }
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var spl = await db.SPLs.Where(x => x.IsDeleted == false && x.ID == id).FirstOrDefaultAsync();
                if (spl == null)
                    return HttpNotFound();
                await db.Entry(spl).Reference(x => x.PRODUCT).LoadAsync();
                return View(spl);
            }
        }
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.products = new SelectList(db.PRODUCTS.Where(x => x.IsDeleted == false).ToList(),"ID", "PNo");
                var spl = await db.SPLs.Where(x => x.IsDeleted == false && x.ID == id).FirstOrDefaultAsync();
                if (spl == null)
                {
                    return HttpNotFound();
                }
                await db.Entry(spl).Reference(x => x.PRODUCT).LoadAsync();
                return View(spl);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SPL spl)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (ModelState.IsValid)
                {
                    var splToUpdate = await db.SPLs.FindAsync(spl.ID);
                    if (splToUpdate != null)
                    {
                        splToUpdate.ProdID = spl.ProdID;
                        splToUpdate.CarModel1 = spl.CarModel1;
                        splToUpdate.Option1 = spl.Option1;
                        splToUpdate.Option1UsageQty = spl.Option1UsageQty;
                        splToUpdate.Option1UsageUnit = spl.Option1UsageUnit;

                        db.Entry(splToUpdate).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();

                        LogHelper.LogToDatabase(User.Identity.Name, "SPLController", $"{spl.PRODUCT.PNo} ga ega {spl.CarModel1} ni tahrirladi");

                        return RedirectToAction("Index");
                    }

                    return View(splToUpdate);
                }
                return View(spl);
            }
        }
        public async Task<ActionResult> Delete(int? Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var spl = await db.SPLs.Where(x => x.IsDeleted == false && x.ID == Id).FirstOrDefaultAsync();
                if (spl == null)
                {
                    return HttpNotFound();
                }
                await db.Entry(spl).Reference(x => x.PRODUCT).LoadAsync();
                return View(spl);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? ID, FormCollection gfs)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    SPL splToDelete = await db.SPLs.Where(x => x.IsDeleted == false && x.ID == ID).FirstOrDefaultAsync();
                    if (splToDelete != null)
                    {
                        splToDelete.IsDeleted = true;
                        try
                        {
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "SPLController", $"{splToDelete.PRODUCT.PNo} ga ega {splToDelete.CarModel1} ni o'chirdi");

                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    else
                        ModelState.AddModelError("", "Bunday spl ma'lumotlari topilmadi.");
                }

                return View();
            }
        }


        //Related to excel upload
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

                                LogHelper.LogToDatabase(User.Identity.Name, "SPLController", $"{newProduct.PNo} - Productni Excell orqali yaratdi");

                                prodID = db.PRODUCTS.Where(x => x.PNo.CompareTo(newProduct.PNo)==0 && x.Name.CompareTo(newProduct.Name)==0).FirstOrDefault().ID;
                            }
                            if (prodID != null)
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

                                LogHelper.LogToDatabase(User.Identity.Name, "SPLController", $"{newSplRecord.PRODUCT.PNo} ga ega {newSplRecord.CarModel1} ni Excell orqali yaratdi");
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
