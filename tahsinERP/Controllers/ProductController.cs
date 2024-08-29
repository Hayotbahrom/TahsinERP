using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using tahsinERP.Models;
using System.Data.Entity;
using DocumentFormat.OpenXml.Office2010.Excel;
namespace tahsinERP.Controllers
{
    public class ProductController : Controller
    {
        private int productPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        private string Pno = "";
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                List<PRODUCT> list = db.PRODUCTS.Include(p => p.UNIT).Where(p => p.IsDeleted == false).ToList();
                ViewBag.CustomerList = new SelectList(db.CUSTOMERS.Where(cs => cs.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.UNIT = new SelectList(db.UNITS.ToList(), "ID", "ShortName");

                return View(list);
            }

        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.UNIT = new SelectList(db.UNITS.ToList(), "ID", "ShortName");
                ViewBag.CustomerList = new SelectList(db.CUSTOMERS.Where(cs => cs.IsDeleted == false).ToList());
                ViewBag.HSCODESS = new SelectList(db.HSCODES.ToList(),"ID", "HSCODE1");

                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PNo, Name, Type, Weight, Length, Width, Height, UnitID, Description, PNo2, PNo3, PNo4, PackID,HSCodeId, IsDeleted")] PRODUCT product)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        // Set IsDeleted to false and save the product to get the ID
                        product.IsDeleted = false;
                        db.PRODUCTS.Add(product);
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "ProductController", $"{product.ID} ID ga ega Productni yaratdi");

                        // Handle image upload
                        var imageFile = Request.Files["productPhotoUpload"]; // Ensure name matches
                        if (imageFile != null && imageFile.ContentLength > 0)
                        {
                            if (imageFile.ContentLength < productPhotoMaxLength)
                            {
                                var photoImage = new PRODUCTIMAGE
                                {
                                    ProdID = product.ID, // Use the product ID
                                    Image = new byte[imageFile.ContentLength],
                                    IsDeleted = false
                                };

                                imageFile.InputStream.Read(photoImage.Image, 0, photoImage.Image.Length);

                                db.PRODUCTIMAGES.Add(photoImage);
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "ProductController", $"{photoImage.ID} ID ga ega ProductImageni yaratdi");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Suratni yuklab bo‘lmadi, u 2 MB dan ortiq. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                                throw new RetryLimitExceededException();
                            }
                        }

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
                ViewBag.CustomerList = new SelectList(db.CUSTOMERS.Where(cs => cs.IsDeleted == false).ToList());
            }
            return View(product);
        }
        public ActionResult Details(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
                
                var product = db.PRODUCTS.Include(x => x.UNIT).Where(x => x.ID == id && x.IsDeleted == false).FirstOrDefault();
                if (product == null)
                    return HttpNotFound();

                // Retrieve the product image
                var productImage = db.PRODUCTIMAGES.FirstOrDefault(img => img.ProdID == id);
                if (productImage != null)
                {
                    ViewBag.ProductImage = Convert.ToBase64String(productImage.Image);
                }
                return View(product);
            }
        }
        public ActionResult Edit(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var product = db.PRODUCTS.Include(x => x.UNIT).SingleOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (product == null)
                {
                    return HttpNotFound();
                }
                ViewBag.CustomerList = new SelectList(db.CUSTOMERS.Where(cs => cs.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.UNIT = new SelectList(db.UNITS.ToList(), "ID", "ShortName", product.UnitID); 

                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PRODUCT product)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (ModelState.IsValid)
                {
                    var productToUpdate = db.PRODUCTS.Find(product.ID);
                    if (productToUpdate != null)
                    {
                        productToUpdate.PNo = product.PNo;
                        productToUpdate.Name = product.Name;
                        productToUpdate.Description = product.Description;
                        productToUpdate.Weight = product.Weight;
                        productToUpdate.Length = product.Length;
                        productToUpdate.Width = product.Width;
                        productToUpdate.Height = product.Height;
                        productToUpdate.UnitID = product.UnitID;
                        productToUpdate.Type = product.Type;
                        productToUpdate.PNo2 = product.PNo2;
                        productToUpdate.PNo3 = product.PNo3;
                        productToUpdate.PNo4 = product.PNo4;

                        var imageFile = Request.Files["productPhotoUpload"]; // Ensure name matches
                        if (imageFile != null && imageFile.ContentLength > 0)
                        {
                            if (imageFile.ContentLength < productPhotoMaxLength)
                            {
                                // Check for existing image
                                var existingImage = db.PRODUCTIMAGES.FirstOrDefault(pi => pi.ProdID == product.ID);

                                if (existingImage != null)
                                {
                                    // Update existing image
                                    existingImage.Image = new byte[imageFile.ContentLength];
                                    imageFile.InputStream.Read(existingImage.Image, 0, existingImage.Image.Length);

                                    LogHelper.LogToDatabase(User.Identity.Name, "ProductController", $"{existingImage.ID} ID ga ega ProductImageni tahrirladi");
                                }
                                else
                                {
                                    // Add new image
                                    var photoImage = new PRODUCTIMAGE
                                    {
                                        ProdID = product.ID, // Use the product ID
                                        Image = new byte[imageFile.ContentLength],
                                        IsDeleted = false
                                    };

                                    imageFile.InputStream.Read(photoImage.Image, 0, photoImage.Image.Length);
                                    db.PRODUCTIMAGES.Add(photoImage);
                                    LogHelper.LogToDatabase(User.Identity.Name, "ProductController", $"{photoImage} ID ga ega ProductImageni yaratdi");
                                }

                                db.SaveChanges();

                            }
                            else
                            {
                                ModelState.AddModelError("", "Suratni yuklab bo‘lmadi, u 2 MB dan ortiq. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                                throw new RetryLimitExceededException();
                            }
                        }

                        db.Entry(productToUpdate).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "ProductController", $"{productToUpdate.ID} ID ga ega Productni tahrirladi");

                        return RedirectToAction("Index");
                    }

                    return View(productToUpdate);
                }
                return View(product);
            }
        }
        public ActionResult Delete(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var product = db.PRODUCTS.Include(x => x.UNIT).SingleOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (product == null)
                {
                    return HttpNotFound();
                }
                ViewBag.CustomerList = new SelectList(db.CUSTOMERS.Where(cs => cs.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.UNIT = new SelectList(db.UNITS.ToList(), "ID", "ShortName", product.UnitID);

                return View(product);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    PRODUCT productToUpdate = db.PRODUCTS.Find(ID);
                    if (productToUpdate != null)
                    {

                        productToUpdate.IsDeleted = true;
                        if (TryUpdateModel(productToUpdate, "", new string[] { "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "ProductController", $"{productToUpdate.ID} ID ga ega Productni o'chirdi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                }
                return View();
            }
        }
        public async Task<ActionResult> Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES maxsulot = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("maxsulot.xlsx") == 0).FirstOrDefault();
                if (maxsulot != null)
                    return File(maxsulot.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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
                                Pno = row["Partnumber"].ToString();

                                PRODUCT product = db.PRODUCTS.Where(p => p.PNo.CompareTo(Pno) == 0 && p.IsDeleted == false).FirstOrDefault();
                                if (product != null)
                                {
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
            ViewBag.DataTable = null;
            ViewBag.DataTableModel = null;
            ViewBag.IsFileUploaded = false;
            ViewBag.Message = "Jadval ma'lumotlari o'chirib yuborildi.";
            return View("UploadWithExcel");
        }
        [HttpPost]
        public async Task<ActionResult> Save(string dataTableModel)
        {
            if (!string.IsNullOrEmpty(dataTableModel))
            {
                /*await Task.Run(() =>
                {*/
                    var tableModel = JsonConvert.DeserializeObject<System.Data.DataTable>(dataTableModel);

                    try
                    {
                        using (DBTHSNEntities db = new DBTHSNEntities())
                        {
                            foreach (DataRow row in tableModel.Rows)
                            {
                                Pno = row["Partnumber"].ToString();

                                PRODUCT product = db.PRODUCTS.Where(p => p.PNo.CompareTo(Pno) == 0 && p.IsDeleted == false).FirstOrDefault();

                                if (product == null)
                                {
                                    PRODUCT newProduct = new PRODUCT();

                                    newProduct.PNo = Pno;
                                    newProduct.Name = row["Name"].ToString();
                                    newProduct.Weight = Double.Parse(row["Weight"].ToString());
                                    newProduct.Length = Double.Parse(row["Length"].ToString());
                                    newProduct.Width = Double.Parse(row["Width"].ToString());
                                    newProduct.Height = Double.Parse(row["Height"].ToString());
                                    //newProduct.Unit = row["Unit"].ToString();
                                    newProduct.Type = row["Type"].ToString();
                                    newProduct.PNo2 = row["PNo2"].ToString();
                                    newProduct.PNo3 = row["PNo3"].ToString();
                                    newProduct.PNo4 = row["PNo4"].ToString();
                                    newProduct.UnitID = 1;
                                    newProduct.IsDeleted = false;

                                    db.PRODUCTS.Add(newProduct);
                                    db.SaveChanges();

                                    LogHelper.LogToDatabase(User.Identity.Name, "ProductController", $"{newProduct.ID} ID ga ega Productni Excell orqali yaratdi");
                                }
                                else
                                {
                                    ViewBag.Message = "Muammo!. Yuklangan faylda ayni vaqtda ma'lumotlar bazasida bor ma'lumot kiritilishga harakat bo'lmoqda.";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                /*});*/
            }

            return RedirectToAction("Index");
        }
    }
}
