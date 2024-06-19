using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class PartController : Controller
    {
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');//new string[4] { "", "KD", "Steel", "Maxalliy" };
        private byte[] avatar;
        private int partPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        private string partNo = "";
        // GET: Part
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var suppliers = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
                if (!string.IsNullOrEmpty(type))
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_by_type_and_supplierID_Result>("EXEC GetPartsInfo_by_type_and_supplierID @Type, @SupplierID", new SqlParameter("@Type", type), new SqlParameter("@SupplierID", supplierID)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name", supplierID);
                    }
                    else
                    {
                        ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_by_type_Result>("EXEC GetPartsInfo_by_type @Type", new SqlParameter("@Type", type)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name");
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_by_supplierID_Result>("EXEC GetPartsInfo_by_supplierID @SupplierID", new SqlParameter("@SupplierID", supplierID)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name", supplierID);
                    }
                    else
                    {
                        ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_Result>("EXEC GetPartsInfo").ToList();
                        ViewBag.SourceList = new SelectList(sources);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name");
                    }
                }

                return View();
            }
        }
        public ActionResult Create()
        {
            PartViewModel partVM = new PartViewModel();
            ViewBag.PartTypes = ConfigurationManager.AppSettings["partTypes"]?.Split(',').ToList() ?? new List<string>();
            ViewBag.Prod_SHops = new SelectList(db.PROD_SHOPS, "ID", "ShopName");
            return View(partVM);
        }
        [HttpPost]
        public ActionResult Create(PartViewModel partVM)
        {
            try
            {
                PART newPart = new PART();
                newPart.PNo = partVM.PNo;
                newPart.PName = partVM.PName;
                newPart.PWeight = partVM.PWeight;
                newPart.PLength = partVM.PLength;
                newPart.PWidth = partVM.PWidth;
                newPart.PHeight = partVM.PHeight;
                newPart.Unit = partVM.Unit;
                newPart.Type = partVM.Type;
                newPart.Description = partVM.Description;
                newPart.IsDeleted = false;
                newPart.Thickness = partVM.Thickness;
                newPart.Grade = partVM.Grade;
                newPart.Gauge = partVM.Gauge;
                newPart.Pitch = partVM.Pitch;
                newPart.Coating = partVM.Coating;
                newPart.Marka = partVM.Marka;
                newPart.Standart = partVM.Standart;
                newPart.IsInHouse = partVM.IsInHouse;
                
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    db.PARTS.Add(newPart);
                    db.SaveChanges();
                newPart.ShopID = partVM.ShopID;

                db.PARTS.Add(newPart);
                db.SaveChanges();

                    if (Request.Files["partPhotoUpload"].ContentLength > 0)
                    {
                        if (Request.Files["partPhotoUpload"].InputStream.Length < partPhotoMaxLength)
                        {
                            PARTIMAGE partImage = new PARTIMAGE();
                            avatar = new byte[Request.Files["partPhotoUpload"].InputStream.Length];
                            Request.Files["partPhotoUploadpart"].InputStream.Read(avatar, 0, avatar.Length);
                            partImage.PartID = newPart.ID;
                            partImage.Image = avatar;

                            db.PARTIMAGES.Add(partImage);
                            db.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("", "Rasmni yuklab bo'lmadi, u 2MBdan kattaroq. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
                            throw new RetryLimitExceededException();
                        }
                    }
                }
            }
                        db.PARTIMAGES.Add(partImage);
                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Rasmni yuklab bo'lmadi, u 2MBdan kattaroq. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
                        throw new RetryLimitExceededException();
                    }
                }
                return RedirectToAction("Index"); 
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Ma'lumotlarni saqlashni iloji bo'lmadi. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
            }
            ViewBag.PartTypes = ConfigurationManager.AppSettings["partTypes"]?.Split(',').ToList() ?? new List<string>();
            ViewBag.Prod_SHops = new SelectList(db.PROD_SHOPS, "ID", "ShopName",partVM.ShopID);
            return View();
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PART part = db.PARTS.Find(ID);
                if (part == null)
                {
                    return HttpNotFound();
                }

            var part = db.PARTS.Find(id);
            if (part == null)
            {
                return HttpNotFound();
            }

            // Populate ViewBag.Prod_Shops for the ShopID dropdown
            ViewBag.Prod_Shops = new SelectList(db.PROD_SHOPS, "ID", "ShopName", part.ShopID);
            ViewBag.PartTypes = ConfigurationManager.AppSettings["partTypes"]?.Split(',').ToList() ?? new List<string>();
            // Populate ViewBag.PartTypes for the Type dropdown
                // Map the properties from the PART entity to a PartViewModel instance
                PartViewModel partViewModel = new PartViewModel();
                partViewModel.PNo = part.PNo;
                partViewModel.PName = part.PName;
                partViewModel.PWeight = part.PWeight;
                partViewModel.PLength = part.PLength;
                partViewModel.PWidth = part.PWidth;
                partViewModel.PHeight = part.PHeight;
                partViewModel.Unit = part.Unit;
                partViewModel.Type = part.Type;
                partViewModel.Description = part.Description;
                partViewModel.Thickness = part.Thickness;
                partViewModel.Grade = part.Grade;
                partViewModel.Gauge = part.Gauge;
                partViewModel.Pitch = part.Pitch;
                partViewModel.Coating = part.Coating;
                partViewModel.Standart = part.Standart;
                partViewModel.Marka = part.Marka;
                partViewModel.IsInHouse = part.IsInHouse;


            PartViewModel partVM = new PartViewModel
            {
                ID = part.ID,
                PNo = part.PNo,
                PName = part.PName,
                PWeight = part.PWeight,
                PLength = part.PLength,
                PWidth = part.PWidth,
                PHeight = part.PHeight,
                Unit = part.Unit,
                Type = part.Type,
                Description = part.Description,
                Thickness = part.Thickness,
                Grade = part.Grade,
                Gauge = part.Gauge,
                Pitch = part.Pitch,
                Coating = part.Coating,
                Marka = part.Marka,
                Standart = part.Standart,
                IsInHouse = part.IsInHouse,
                ShopID = part.ShopID ?? 0,
                ShopName = db.PROD_SHOPS.Where(s => s.ID == part.ShopID).Select(s => s.ShopName).FirstOrDefault()
            };

            return View(partVM);
        }
                return View(partViewModel);
            }
            // Fetch the part entity from the database
        }





        [HttpPost]
        public ActionResult Edit(PartViewModel partVM)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var partToUpdate = db.PARTS.Find(partVM.ID);
                    if (partToUpdate != null)
                    {
                        partToUpdate.PNo = partVM.PNo;
                        partToUpdate.PName = partVM.PName;
                        partToUpdate.Description = partVM.Description;
                        partToUpdate.PWeight = partVM.PWeight;
                        partToUpdate.PLength = partVM.PLength;
                        partToUpdate.PWidth = partVM.PWidth;
                        partToUpdate.PHeight = partVM.PHeight;
                        partToUpdate.Unit = partVM.Unit;
                        partToUpdate.Type = partVM.Type;
                        partToUpdate.IsDeleted = false;
                        partToUpdate.Thickness = partVM.Thickness;
                        partToUpdate.Grade = partVM.Grade;
                        partToUpdate.Gauge = partVM.Gauge;
                        partToUpdate.Pitch = partVM.Pitch;
                        partToUpdate.Coating = partVM.Coating;
                        partToUpdate.Standart = partVM.Standart;
                        partToUpdate.Marka = partVM.Marka;
                        partToUpdate.IsInHouse = partVM.IsInHouse;
            try
            {
                if (ModelState.IsValid)
                {
                    var part = db.PARTS.Find(partVM.ID);
                    if (part == null)
                    {
                        return HttpNotFound();
                    }

                    part.PNo = partVM.PNo;
                    part.PName = partVM.PName;
                    part.PWeight = partVM.PWeight;
                    part.PLength = partVM.PLength;
                    part.PWidth = partVM.PWidth;
                    part.PHeight = partVM.PHeight;
                    part.Unit = partVM.Unit;
                    part.Type = partVM.Type;
                    part.Description = partVM.Description;
                    part.IsDeleted = false;
                    part.Thickness = partVM.Thickness;
                    part.Grade = partVM.Grade;
                    part.Gauge = partVM.Gauge;
                    part.Pitch = partVM.Pitch;
                    part.Coating = partVM.Coating;
                    part.Marka = partVM.Marka;
                    part.Standart = partVM.Standart;
                    part.IsInHouse = partVM.IsInHouse;
                    part.ShopID = partVM.ShopID;

                    db.Entry(part).State = EntityState.Modified;
                    db.SaveChanges();

                        var imageFile = Request.Files["partPhotoUpload"];
                        if (imageFile != null && imageFile.ContentLength > 0)
                        {
                            if (imageFile.ContentLength < partPhotoMaxLength)
                            {
                                var existingImage = db.PARTIMAGES.FirstOrDefault(pi => pi.PartID == partVM.ID);
                    if (Request.Files["partPhotoUpload"] != null && Request.Files["partPhotoUpload"].ContentLength > 0)
                    {
                        if (Request.Files["partPhotoUpload"].InputStream.Length < partPhotoMaxLength)
                        {
                            var existingPartImage = db.PARTIMAGES.SingleOrDefault(pi => pi.PartID == part.ID);
                            byte[] avatar = new byte[Request.Files["partPhotoUpload"].InputStream.Length];
                            Request.Files["partPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);

                                if (existingImage != null)
                                {
                                    existingImage.Image = new byte[imageFile.ContentLength];
                                    imageFile.InputStream.Read(existingImage.Image, 0, existingImage.Image.Length);
                                }
                                else
                                {
                                    var photoImage = new PARTIMAGE
                                    {
                                        PartID = partVM.ID,
                                        Image = new byte[imageFile.ContentLength],
                                        IsDeleted = false
                                    };
                            if (existingPartImage != null)
                            {
                                existingPartImage.Image = avatar;
                            }
                            else
                            {
                                PARTIMAGE partImage = new PARTIMAGE
                                {
                                    PartID = part.ID,
                                    Image = avatar
                                };
                                db.PARTIMAGES.Add(partImage);
                            }

                                    imageFile.InputStream.Read(photoImage.Image, 0, photoImage.Image.Length);
                                    db.PARTIMAGES.Add(photoImage);
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Suratni yuklab bo‘lmadi, u 2 MB dan ortiq. Qayta urinib ko'ring va muammo davom etsa, tizim administratoriga murojaat qiling.");
                                throw new RetryLimitExceededException();
                            }
                        }
                            db.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("", "Rasmni yuklab bo'lmadi, u 2MBdan kattaroq. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
                            throw new RetryLimitExceededException();
                        }
                    }

                        db.Entry(partToUpdate).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }

                }
            }
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ma'lumotlarni saqlashni iloji bo'lmadi: {ex.Message}. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
            }

            ViewBag.PartTypes = ConfigurationManager.AppSettings["partTypes"]?.Split(',').ToList() ?? new List<string>();
            ViewBag.Prod_SHops = new SelectList(db.PROD_SHOPS, "ID", "ShopName", partVM.ShopID);
            return View(partVM);
        }


        public ActionResult Delete(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var part = db.PARTS.Find(ID);
                if (part == null)
                    return HttpNotFound();

                return View(part);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfc)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PART partToDelete = db.PARTS.Find(ID);
                    if (partToDelete != null)
                    {
                        partToDelete.IsDeleted = true;
                        if (TryUpdateModel(partToDelete, "", new string[] { "IsDeleted" }))
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
                    else
                    {
                        ModelState.AddModelError("", "Bunday detall topilmadi.");
                    }
                }

            }
            return View();
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PartViewModel partvm = new PartViewModel();
                PART selectedPart = db.PARTS.Find(ID);
                if (selectedPart == null)
                    return HttpNotFound();
                else
                {
                    partvm.ID = selectedPart.ID;
                    partvm.PNo = selectedPart.PNo;
                    partvm.PName = selectedPart.PName;
                    partvm.Standart = selectedPart.Standart;
                    partvm.PWidth = selectedPart.PWidth;
                    partvm.PLength = selectedPart.PLength;
                    partvm.PHeight = selectedPart.PHeight;
                    partvm.Thickness = selectedPart.Thickness;
                    partvm.Description = selectedPart.Description;
                    partvm.Gauge = selectedPart.Gauge;
                    partvm.Coating = selectedPart.Coating;
                    partvm.Marka = selectedPart.Marka;
                    partvm.Grade = selectedPart.Grade;
                    partvm.IsInHouse = selectedPart.IsInHouse;
                    partvm.Pitch = selectedPart.Pitch;
                    partvm.Unit = selectedPart.Unit;
                }
                PARTIMAGE partimage = db.PARTIMAGES.Where(ui => ui.PartID == selectedPart.ID).FirstOrDefault();
                if (partimage != null)
                {
                    ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String(partimage.Image, 0, partimage.Image.Length);
                }
                return View(partvm);
            }
        }

            var part = db.PARTS.Find(id);
            if (part == null)
            {
                return HttpNotFound();
            }

            var shop = db.PROD_SHOPS.Find(part.ShopID);
            string shopName = shop != null ? shop.ShopName : "Unknown";

            PartViewModel partVM = new PartViewModel
            {
                ID = part.ID,
                PNo = part.PNo,
                PName = part.PName,
                PWeight = part.PWeight,
                PLength = part.PLength,
                PWidth = part.PWidth,
                PHeight = part.PHeight,
                Unit = part.Unit,
                Type = part.Type,
                Description = part.Description,
                Thickness = part.Thickness,
                Grade = part.Grade,
                Gauge = part.Gauge,
                Pitch = part.Pitch,
                Coating = part.Coating,
                Marka = part.Marka,
                Standart = part.Standart,
                IsInHouse = part.IsInHouse,
                ShopID = part.ShopID ?? 0, // Default to 0 if null
                ShopName = shopName // Set the shop name
            };

            // Assuming the image is stored as a base64 string in the viewbag
            var partImage = db.PARTIMAGES.FirstOrDefault(pi => pi.PartID == part.ID);
            if (partImage != null)
            {
                ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String(partImage.Image);
            }

            return View(partVM);
        }


        public ActionResult UploadWithExcel()
        {
            ViewBag.IsFileUploaded = false;
            return View();
        }
        public ActionResult Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES detal = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("detallar.xlsx") == 0).FirstOrDefault();
                if (detal != null)
                    return File(detal.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                return View();
            }
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
                                    var cellText = worksheet.Cells[row, col].Text;

                                    // Assuming 'PartName' is the name of the column that may contain en-dashes or em-dashes
                                    if (dataTable.Columns[col - 1].ColumnName == "PartName")
                                    {
                                        // Replace en-dash and em-dash with a standard hyphen
                                        cellText = cellText.Replace('–', '-').Replace('—', '-');
                                    }
                                    cellText = new string(cellText.Where(c => !char.IsControl(c)).ToArray()).Trim();

                                    dataRow[col - 1] = cellText;
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
                                partNo = row["Partnumber"].ToString();

                                PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                if (part != null)
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
                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        foreach (DataRow row in tableModel.Rows)
                        {
                            partNo = row["Partnumber"].ToString();

                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();

                            if (part == null)
                            {
                                PART newPart = new PART();
                                newPart.PNo = row["Partnumber"].ToString();
                                newPart.PName = row["PartName"].ToString();
                                newPart.PWeight = Double.Parse(row["Weight"].ToString());
                                newPart.PLength = Double.Parse(row["Length"].ToString());
                                newPart.PWidth = Double.Parse(row["Width"].ToString());
                                newPart.PHeight = Double.Parse(row["Height"].ToString());
                                newPart.Thickness = Double.Parse(row["Thickness"].ToString());
                                newPart.Grade = row["Grade"].ToString();
                                newPart.Gauge = Double.Parse(row["Gauge"].ToString());
                                newPart.Pitch = Double.Parse(row["Pitch"].ToString());
                                newPart.Coating = row["Coating"].ToString();
                                newPart.Marka = row["Standart"].ToString();
                                newPart.Standart = row["Standart"].ToString();
                                newPart.Unit = row["Unit"].ToString();
                                if (row["InHouse?"].ToString().CompareTo("Yes") == 0)
                                    newPart.IsInHouse = true;
                                else
                                    newPart.IsInHouse = false;
                                newPart.IsDeleted = false;

                                db.PARTS.Add(newPart);
                                db.SaveChanges();
                            }
                            else
                            {
                                ViewBag.Message = "Muammo!. Yuklangan faylda ayni vaqtda ma'lumotlar bazasida bor ma'lumot kiritilishga harakat bo'lmoqda.";
                            }
                        }
                    }
                }
                catch (JsonReaderException jex)
                {
                    // Log the detailed exception message and stack trace
                    Console.WriteLine("JSON Reader Exception: " + jex.Message);
                    Console.WriteLine(jex.StackTrace);
                    // Handle the exception
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