using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class PartController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');//new string[4] { "", "KD", "Steel", "Maxalliy" };
        //private byte[] avatar;
        private int partPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        private string partNo = "";

        // GET: Part
        public ActionResult GetAllParts()
        {
            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                var parts = db1.PARTS
                    .Where(p => p.IsDeleted == false).ToList();
                return View(parts); 
            }
        }
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
            ViewBag.Prod_Shops = new SelectList(db.SHOPS, "ID", "ShopName");
            return View(partVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                newPart.ShopID = partVM.ShopID;

                db.PARTS.Add(newPart);
                db.SaveChanges();


                SHOP pROD_SHOPS = db.SHOPS.Where(x => x.ID.Equals(partVM.ShopID)).FirstOrDefault();
                if (pROD_SHOPS != null)
                {
                    bool exists = db.Database.SqlQuery<int>(
                "SELECT COUNT(*) FROM Prod_Shops_Parts WHERE ShopId = @ShopId AND PartId = @PartId",
                new SqlParameter("@ShopId", pROD_SHOPS.ID),
                new SqlParameter("@PartId", newPart.ID)
            ).FirstOrDefault() > 0;
                    if (!exists)
                    {

                        int shopId = db.Database.SqlQuery<int>("Select shopId from Prod_Shops_Parts where ShopId=" + pROD_SHOPS.ID + "and PartId = " + newPart.ID + "").FirstOrDefault();
                        if (shopId != pROD_SHOPS.ID)
                        {
                            int noOfRowInserted = db.Database.ExecuteSqlCommand("Insert Into Prod_Shops_Parts ([ShopId],[PartId]) Values(" + pROD_SHOPS.ID + "," + newPart.ID + ")");
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bu Shop va Part kombinatsiyasi allaqachon mavjud.");
                    }
                }
                if (Request.Files["partPhotoUpload"].ContentLength > 0)
                {
                    if (Request.Files["partPhotoUpload"].InputStream.Length < partPhotoMaxLength)
                    {
                        PARTIMAGE partImage = new PARTIMAGE();
                        byte[] avatar = new byte[Request.Files["partPhotoUpload"].InputStream.Length];
                        Request.Files["partPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
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

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, "Ma'lumotlarni saqlashni iloji bo'lmadi. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
            }

            ViewBag.PartTypes = ConfigurationManager.AppSettings["partTypes"]?.Split(',').ToList() ?? new List<string>();
            ViewBag.Prod_Shops = new SelectList(db.SHOPS, "ID", "ShopName", partVM.ShopID);
            return View(partVM);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var part = db.PARTS.Find(id);
            if (part == null)
            {
                return HttpNotFound();
            }

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
                ShopID = part.ShopID ?? 0
            };

            ViewBag.PartTypes = ConfigurationManager.AppSettings["partTypes"]?.Split(',').ToList() ?? new List<string>();
            ViewBag.Prod_Shops = new SelectList(db.SHOPS, "ID", "ShopName", part.ShopID);

            return View(partVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PartViewModel partVM)
        {
            try
            {
                var partToUpdate = db.PARTS.Find(partVM.ID);
                if (partToUpdate == null)
                {
                    return HttpNotFound();
                }

                partToUpdate.PNo = partVM.PNo;
                partToUpdate.PName = partVM.PName;
                partToUpdate.PWeight = partVM.PWeight;
                partToUpdate.PLength = partVM.PLength;
                partToUpdate.PWidth = partVM.PWidth;
                partToUpdate.PHeight = partVM.PHeight;
                partToUpdate.Unit = partVM.Unit;
                partToUpdate.Type = partVM.Type;
                partToUpdate.Description = partVM.Description;
                partToUpdate.Thickness = partVM.Thickness;
                partToUpdate.Grade = partVM.Grade;
                partToUpdate.Gauge = partVM.Gauge;
                partToUpdate.Pitch = partVM.Pitch;
                partToUpdate.Coating = partVM.Coating;
                partToUpdate.Marka = partVM.Marka;
                partToUpdate.Standart = partVM.Standart;
                partToUpdate.IsInHouse = partVM.IsInHouse;
                partToUpdate.ShopID = partVM.ShopID;

                db.Entry(partToUpdate).State = EntityState.Modified;
                db.SaveChanges();

                // Update the many-to-many table Prod_Shop_Parts
                db.Database.ExecuteSqlCommand("DELETE FROM Prod_Shops_Parts WHERE PartID = {0}", partToUpdate.ID);
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO Prod_Shops_Parts (PartID, ShopID) VALUES ({0}, {1})", partToUpdate.ID, partVM.ShopID);

                if (Request.Files["partPhotoUpload"].ContentLength > 0)
                {
                    if (Request.Files["partPhotoUpload"].InputStream.Length < partPhotoMaxLength)
                    {
                        var partImage = db.PARTIMAGES.FirstOrDefault(pi => pi.PartID == partToUpdate.ID);
                        if (partImage == null)
                        {
                            partImage = new PARTIMAGE();
                            db.PARTIMAGES.Add(partImage);
                        }

                        byte[] avatar = new byte[Request.Files["partPhotoUpload"].InputStream.Length];
                        Request.Files["partPhotoUpload"].InputStream.Read(avatar, 0, avatar.Length);
                        partImage.PartID = partToUpdate.ID;
                        partImage.Image = avatar;

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
            ViewBag.Prod_Shops = new SelectList(db.SHOPS, "ID", "ShopName", partVM.ShopID);
            return View(partVM);
        }
        public ActionResult Delete(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var part = db.PARTS.Find(ID);
            if (part == null)
                return HttpNotFound();

            return View(part);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfc)
        {
            if (ModelState.IsValid)
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
            return View();
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var part = db.PARTS.Find(id);
            ViewBag.contractList = ContractsOfParts(part.ID);
            if (part == null)
            {
                return HttpNotFound();
            }

            var shop = db.SHOPS.Find(part.ShopID);
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
        public List<ContractsOfPartsViewModel> ContractsOfParts(int? partID)
        {
            if (partID == null)
            {
                return null;
            }

            using (DBTHSNEntities dbContext = new DBTHSNEntities())
            {
                var result = dbContext.P_CONTRACTS
                    .Join(dbContext.P_CONTRACT_PARTS, pc => pc.ID, pcp => pcp.ContractID, (pc, pcp) => new { pc, pcp })
                    .Join(dbContext.SUPPLIERS, combined => combined.pc.SupplierID, s => s.ID, (combined, s) => new { combined.pc, combined.pcp, s })
                    .Where(x => x.pcp.PartID == partID && x.pc.IsDeleted == false)
                    .Select(x => new ContractsOfPartsViewModel
                    {
                        ContractNo = x.pc.ContractNo,
                        ContractID = x.pc.ID,
                        SupplierName = x.s.Name,
                        SupplierID = x.s.ID,
                        IssuedDate = x.pc.IssuedDate,
                        DueDate = x.pc.DueDate
                    })
                    .ToList();

                return result;
            }
        }
        public ActionResult UploadWithExcel()
        {
            ViewBag.IsFileUploaded = false;
            return View();
        }
        public ActionResult Download()
        {
            SAMPLE_FILES detal = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("detallar.xlsx") == 0).FirstOrDefault();
            if (detal != null)
                return File(detal.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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