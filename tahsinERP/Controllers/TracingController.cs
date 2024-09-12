using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class TracingController : Controller
    {
        // GET: Tracing
        public async Task<ActionResult> Index(DateTime? date = null)
        {
            date = date ?? DateTime.Now.Date;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = await db.TRACINGS
                    .Include(p => p.P_INVOICE_PACKINGLISTS)
                    .Where(p => p.IsDeleted == false && p.IssueDateTime == date)
                    .GroupBy(t => t.P_INVOICE_PACKINGLISTS.TransportNo)
                    .Select(g => new TracingViewModel
                    {
                        TransportNo = g.Key,
                        LastIssueDateTime = g.Max(t => t.IssueDateTime),
                        LastTracing = g.OrderByDescending(t => t.IssueDateTime).FirstOrDefault(),
                        Tracings = g.OrderBy(t => t.IssueDateTime).ToList()
                    })
                    .ToListAsync();

                // Ensure the list is not null
                if (list == null)
                {
                    list = new List<TracingViewModel>();
                }

                ViewBag.Date = date.Value.ToString("dd-MM-yyyy");

                return View(list);
            }
        }


        public async Task<JsonResult> GetInvoiceNo_Supplier(int packingListID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // PackingListni olish
                var packingList = await db.P_INVOICE_PACKINGLISTS
                                          .Where(x => x.IsDeleted == false && x.ID == packingListID)
                                          .Select(x => new { x.InvoiceID })
                                          .FirstOrDefaultAsync();

                // Agar packingList topilmasa, null qaytarish
                if (packingList == null)
                {
                    return Json(new { success = false, message = "Packing list not found." }, JsonRequestBehavior.AllowGet);
                }

                // Invoice'ni olish
                var invoice = await db.P_INVOICES
                                      .Where(x => x.IsDeleted == false && x.ID == packingList.InvoiceID)
                                      .Select(x => new { x.InvoiceNo, x.SupplierID })
                                      .FirstOrDefaultAsync();

                // Agar invoice topilmasa, null qaytarish
                if (invoice == null)
                {
                    return Json(new { success = false, message = "Invoice not found." }, JsonRequestBehavior.AllowGet);
                }

                // Supplier'ni olish
                var supplier = await db.SUPPLIERS
                                       .Where(x => x.IsDeleted == false && x.ID == invoice.SupplierID)
                                       .Select(x => new { x.Name })
                                       .FirstOrDefaultAsync();

                // Agar supplier topilmasa, null qaytarish
                if (supplier == null)
                {
                    return Json(new { success = false, message = "Supplier not found." }, JsonRequestBehavior.AllowGet);
                }

                // Natijani JSON ko'rinishida qaytarish
                return Json(new
                {
                    success = true,
                    invoiceNo = invoice.InvoiceNo,
                    supplierName = supplier.Name
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Create()
        {
            LoadViewBags();

            TRACING model = new TRACING();
            model.IssueDateTime = DateTime.Now.ToShortDateString().AsDateTime();

            return View(model);
        }
        private void LoadViewBags()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Get the list of packing lists that are not deleted
                var packingLists = db.P_INVOICE_PACKINGLISTS
                    .Where(p => p.IsDeleted == false)
                    .ToList();

                // Group by TransportNo and select the first item from each group to ensure distinct TransportNo values
                var distinctPackingLists = packingLists
                    .GroupBy(p => p.TransportNo)
                    .Select(g => g.First())
                    .ToList();

                // Create the SelectList with distinct TransportNo
                ViewBag.packingList = new SelectList(distinctPackingLists, "ID", "TransportNo");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TRACING tracing)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                bool checkExistTracing = checkForTodaysInput(tracing.PackingListID, tracing.IssueDateTime); //await db.TRACINGS.Where(x => x.PackingListID == tracing.PackingListID && x.IssueDateTime.ToShortDateString().CompareTo(tracing.IssueDateTime.ToShortDateString()) == 0).FirstOrDefaultAsync();
                string transportNo = db.P_INVOICE_PACKINGLISTS.Where(pck => pck.ID == tracing.PackingListID).Select(pck => pck.TransportNo).FirstOrDefault();
                if (checkExistTracing)
                {
                    ModelState.AddModelError("", "Bu sana bilan ma'lumot allaqachon kiritilgan. Qaytadan urunib ko'ring!");
                }
                try
                {
                    if (ModelState.IsValid)
                    {
                        // Set IsDeleted to false and save the tracing to get the ID
                        tracing.IsDeleted = false;
                        db.TRACINGS.Add(tracing);
                        setPackingListInTransit(tracing.PackingListID);
                        await db.SaveChangesAsync();

                        LogHelper.LogToDatabase(User.Identity.Name, "TracingController", $"{transportNo} uchun Tracking {tracing.ID} ID ga ega Tracingni yaratdi");

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }

            }
            LoadViewBags();
            return View(tracing);
        }
        private void setPackingListInTransit(int packingListID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                P_INVOICE_PACKINGLISTS packingList = db.P_INVOICE_PACKINGLISTS.Where(x => x.ID == packingListID).FirstOrDefault();
                if (packingList != null)
                {
                    packingList.InTransit = true;
                    db.Entry(packingList).State = EntityState.Modified;
                    db.SaveChanges();

                    LogHelper.LogToDatabase(User.Identity.Name, "TracingController", $"{packingList.PackingListNo} - PInvoicePackingListning InTransitni yoqdi");
                }
            }
        }

        private bool checkForTodaysInput(int packingListID, DateTime issueDateTime)
        {
            DateTime day = issueDateTime.Date;
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                TRACING tracing = db.TRACINGS.Where(tr => tr.PackingListID == packingListID && tr.IssueDateTime == day).FirstOrDefault();
                if (tracing != null)
                    return true;
                else
                    return false;
            }
        }
        public async Task<ActionResult> Details(int? id)
        {
            if (id is null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var existTracingTransportNo = await db.TRACINGS
                                                    .Include(p => p.P_INVOICE_PACKINGLISTS)
                                                    .Where(p => p.IsDeleted == false && p.PackingListID == id)
                                                    .FirstOrDefaultAsync();

                var tracingList = await db.TRACINGS
                    .Include(p => p.P_INVOICE_PACKINGLISTS)
                    .Where(p => p.IsDeleted == false && p.P_INVOICE_PACKINGLISTS.TransportNo.CompareTo(existTracingTransportNo.P_INVOICE_PACKINGLISTS.TransportNo) == 0)
                    .ToListAsync();

                if (tracingList == null || tracingList.Count == 0)
                    return HttpNotFound();

                return View(tracingList);
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

                var tracing = await db.TRACINGS.FindAsync(id);
                if (tracing == null)
                {
                    return HttpNotFound();
                }
                ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync(), "ID", "PackingListNo");

                return View(tracing);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TRACING tracing)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var tracingToUpdate = await db.TRACINGS.FindAsync(tracing.ID);
                    if (tracingToUpdate != null)
                    {
                        tracingToUpdate.PackingListID = tracing.PackingListID;
                        tracingToUpdate.ActualLocation = tracing.ActualLocation;
                        tracingToUpdate.ActualDistanceToDestination = tracing.ActualDistanceToDestination;
                        tracingToUpdate.IssueDateTime = tracing.IssueDateTime;
                        tracingToUpdate.ETA = tracing.ETA;

                        db.Entry(tracingToUpdate).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        LogHelper.LogToDatabase(User.Identity.Name, "TracingController", $"{tracing.P_INVOICE_PACKINGLISTS.PackingListNo} uchun Tracking {tracing.ID} ID ga ega Tracingni tahrirladi");

                        return RedirectToAction("Index");
                    }

                    return View(tracingToUpdate);
                }
                return View(tracing);
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
                var tracing = await db.TRACINGS.FindAsync(Id);
                if (tracing == null)
                {
                    return HttpNotFound();
                }
                ViewBag.packingList = new SelectList(await db.P_INVOICE_PACKINGLISTS.Where(p => p.IsDeleted == false).ToListAsync(), "ID", "PackingListNo");

                return View(tracing);
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
                    TRACING tracingToDelete = await db.TRACINGS.FindAsync(ID);
                    if (tracingToDelete != null)
                    {

                        tracingToDelete.IsDeleted = true;

                        try
                        {
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "TracingController", $"{tracingToDelete.P_INVOICE_PACKINGLISTS.PackingListNo} uchun Tracking {tracingToDelete.ID} ID ga ega Tracingnini o'chirdi");

                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                }
                return View();
            }
        }

        public ActionResult Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES sampleFile = db.SAMPLE_FILES.FirstOrDefault(s => s.FileName.CompareTo("tracings.xlsx") == 0);
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

                            // Add columns to DataTable
                            for (int col = 1; col <= colCount; col++)
                            {
                                dataTable.Columns.Add(worksheet.Cells[1, col].Text);
                            }

                            // Add rows to DataTable
                            for (int row = 2; row <= rowCount; row++)
                            {
                                var dataRow = dataTable.NewRow();

                                // Parse IssuedDate
                                DateTime issueDate;
                                if (!DateTime.TryParse(worksheet.Cells[row, 4].Text, out issueDate))
                                {
                                    ViewBag.Message = $"Invalid date format in row {row}. Please check the IssuedDate column.";
                                    return View("UploadWithExcel");
                                }

                                // Fill the DataRow
                                for (int col = 1; col <= colCount; col++)
                                {
                                    dataRow[col - 1] = worksheet.Cells[row, col].Text;
                                }
                                dataTable.Rows.Add(dataRow);
                            }
                            if (CheckForExstenceOfTransportNo(dataTable))
                            {
                                if (CheckForExistenceOfSameTracingInOneDay(dataTable))
                                {
                                    ViewBag.DataTable = dataTable;
                                    ViewBag.DataTableModel = JsonConvert.SerializeObject(dataTable);
                                    ViewBag.IsFileUploaded = true;

                                    /*using (DBTHSNEntities db = new DBTHSNEntities())
                                    {
                                        foreach (DataRow row in dataTable.Rows) 
                                        {

                                        }
                                    }*/
                                }
                                else
                                {
                                    var message = "";
                                    foreach (KeyValuePair<string, DateTime> word in transportRecords)
                                    {
                                        message += (word.Key + word.Value.ToString());
                                        ViewBag.Message = "Ushbu tracing faylida kiritilgan transportNo va IssueDate" + message + "bir sanada ko'p marta kiritib bo'lmaydi";
                                    }
                                    return View("UploadWithExcel");
                                }
                            }
                            else
                            {
                                var message = "";
                                foreach (var word in missingTransportNos)
                                {
                                    message += word + ", ";
                                }
                                ViewBag.Message = "Ushbu tracing faylida kiritilgan TransportNo lar: " + message + " tizim bazasida mavjud emas, avval Packinglistlar ichida borligiga ishonch hosil qiling.";
                                return View("UploadWithExcel");
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
        private Dictionary<string, DateTime> transportRecords = new Dictionary<string, DateTime>();
        private bool CheckForExistenceOfSameTracingInOneDay(DataTable dataTable)
        {
            bool flag = false;
            if (dataTable != null)
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string transportNo = row["Transport No."].ToString();
                        DateTime issueDate = Convert.ToDateTime(row["IssuedDate"]);

                        // Agar transport raqami va issue date kombinatsiyasi allaqachon bor bo'lsa, true qaytaramiz
                        if (transportRecords.ContainsKey(transportNo) && transportRecords[transportNo] == issueDate)
                        {
                            flag = true;
                        }
                        // Agar hali yo'q bo'lsa, yangi entry qo'shamiz
                        else
                        {
                            transportRecords[transportNo] = issueDate;
                        }
                        //todo logic
                    }
                }

            return flag;
        }


        private List<string> missingTransportNos = new List<string>();
        private bool CheckForExstenceOfTransportNo(DataTable dataTable)
        {
            bool flag = false;
            if (dataTable != null)
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string transportNo = row["Transport No."].ToString();
                        if (db.P_INVOICE_PACKINGLISTS.Where(x => x.IsDeleted == false && x.TransportNo.CompareTo(transportNo) == 0).Any())
                            flag = true;
                        else
                            if (!missingTransportNos.Contains(transportNo))
                            missingTransportNos.Add(transportNo);
                    }
                    if (missingTransportNos.Count > 0)
                        return false;
                    else
                        return flag;
                }
            else
                return flag;
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
                            string transportNo = row["Transport No."].ToString();
                            DateTime issueDate = DateTime.Parse(row["IssuedDate"].ToString());
                            var packinglistID = db.P_INVOICE_PACKINGLISTS.Where(x => x.IsDeleted == false && x.TransportNo.CompareTo(transportNo) == 0).FirstOrDefault().ID;
                            if (db.P_INVOICE_PACKINGLISTS.Any(x => x.TransportNo == transportNo && x.IsDeleted == false))
                            {
                                var existTracing = db.TRACINGS.FirstOrDefault(x => x.IsDeleted == false && x.P_INVOICE_PACKINGLISTS.TransportNo.CompareTo(transportNo) == 0 && x.IssueDateTime == issueDate);
                                if (existTracing is null)
                                {
                                    TRACING newTracing = new TRACING
                                    {
                                        PackingListID = packinglistID,
                                        ActualLocation = Convert.ToString(row["Actual location"]),
                                        ActualDistanceToDestination = Convert.ToDouble(row["ActualDistanceToDestination"]),
                                        ETA = Convert.ToDateTime(row["ETA"]),
                                        ETD = Convert.ToDateTime(row["ETD"]),
                                        IssueDateTime = Convert.ToDateTime(row["IssuedDate"])
                                    };

                                    db.TRACINGS.Add(newTracing);
                                    db.SaveChanges();

                                    LogHelper.LogToDatabase(User.Identity.Name, "TracingController", $"{newTracing.ID} ID ga ega Productni Excell orqali yaratdi");
                                }
                                else
                                {
                                    ModelState.AddModelError("", "bunday transportNo kiritilgan sanada mavjud.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "bunday transportNo packinglist da topilmadi, ma'lumotlarni qaytadan ko'rib chiqing.");
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