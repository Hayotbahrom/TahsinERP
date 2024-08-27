using DocumentFormat.OpenXml.Office2010.Excel;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class PInvoiceController : Controller
    {
        private string supplierName, invoiceNo, orderNo, partNo = "";
        private string[] sources;
        public PInvoiceController()
        {
            sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
            sources = sources.Where(x => !x.Equals("InHouse", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        // GET: PInvoice
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var suppliers = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
                if (!string.IsNullOrEmpty(type))
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.P_INVOICES.Include(x => x.P_ORDERS).Where(s => s.IsDeleted == false && s.SupplierID == supplierID && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.Type.CompareTo(type) == 0), "ID", "Name", supplierID);
                    }
                    else
                    {
                        ViewBag.partList = db.P_INVOICES.Include(x => x.P_ORDERS).Where(s => s.IsDeleted == false && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.Type.CompareTo(type) == 0), "ID", "Name");
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.P_INVOICES.Include(x => x.P_ORDERS).Where(s => s.IsDeleted == false && s.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name", supplierID);
                    }
                    else
                    {
                        ViewBag.partList = db.P_INVOICES.Include(x => x.P_ORDERS).Where(s => s.IsDeleted == false).ToList();
                        ViewBag.SourceList = new SelectList(sources);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name");
                    }
                }
                return View();
            }
        }

        /*
                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult Create(PInvoiceViewModel model)
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        var isSameContract = db.P_ORDERS
                            .Include(x => x.SUPPLIER)
                            .FirstOrDefault(p => p.IsDeleted == false && p.ID == model.OrderID);

                        if (isSameContract == null || model.SupplierID != isSameContract.SupplierID)
                        {
                            ModelState.AddModelError("", "Ta'minotchi va buyurtma ta'minotchisi bir xil emas!");
                            PopulateViewBags();
                            return View(model);
                        }

                        P_INVOICES invoice = new P_INVOICES
                        {
                            InvoiceNo = model.InvoiceNo,
                            OrderID = model.OrderID,
                            SupplierID = model.SupplierID,
                            Currency = model.Currency,
                            InvoiceDate = model.InvoiceDate,
                            CompanyID = 1,
                            IsDeleted = false
                        };

                        db.P_INVOICES.Add(invoice);
                        db.SaveChanges();   

                        int newInvoiceID = invoice.ID;

                        foreach (var item in model.Parts)
                        {
                            var newPart = new P_INVOICE_PARTS
                            {
                                InvoiceID = newInvoiceID,
                                PartID = item.PartID,
                                Quantity = item.Quantuty,
                                UnitID = item.UnitID,
                                Price = item.Price
                            };

                            db.P_INVOICE_PARTS.Add(newPart);
                        }

                        db.SaveChanges();

                        var userEmail = User.Identity.Name;
                        LogHelper.LogToDatabase(userEmail, "PInvoiceController", "Create[Post]");
                        return RedirectToAction("Index");
                    }
                }*/
        public JsonResult GetOrdersBySupplier(int supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contracts = db.P_ORDERS
                    .Where(x => x.IsDeleted == false && x.SupplierID == supplierID)
                    .Select(x => new { x.ID, x.OrderNo })
                    .ToList();

                return Json(contracts.Select(c => new SelectListItem { Value = c.ID.ToString(), Text = c.OrderNo }), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPartsByOrder(int orderID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var parts = db.P_ORDER_PARTS
                    .Where(po => po.OrderID == orderID)
                    .Select(x => new { x.ID, x.PART.PNo })
                    .ToList();

                return Json(parts.Select(c => new SelectListItem { Value = c.ID.ToString(), Text = c.PNo }), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create()
        {
            PopulateViewBags();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PInvoiceViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var isSameOrder = db.P_ORDERS
                    .Include(x => x.SUPPLIER)
                    .FirstOrDefault(p => p.IsDeleted == false && p.ID == model.OrderID);

                if (isSameOrder == null || model.SupplierID != isSameOrder.SupplierID)
                {
                    ModelState.AddModelError("", "Siz tanlagan ta'minotchi va buyurtma ta'minotchisi bir xil emas!");
                    PopulateViewBags();
                    return View(model);
                }

                P_INVOICES invoice = new P_INVOICES
                {
                    InvoiceNo = model.InvoiceNo,
                    OrderID = model.OrderID,
                    SupplierID = model.SupplierID,
                    Currency = model.Currency,
                    InvoiceDate = model.InvoiceDate,
                    CompanyID = 1,
                    IsDeleted = false
                };

                db.P_INVOICES.Add(invoice);
                db.SaveChanges();

                int newInvoiceID = invoice.ID;

                foreach (var item in model.Parts)
                {
                    var newPart = new P_INVOICE_PARTS
                    {
                        InvoiceID = newInvoiceID,
                        PartID = item.PartID,
                        Quantity = item.Quantity,
                        UnitID = item.UnitID,
                        Price = item.Price
                    };

                    db.P_INVOICE_PARTS.Add(newPart);
                }

                db.SaveChanges();
                if (Request.Files["docUpload"].ContentLength > 0)
                {
                    if (Request.Files["docUpload"].InputStream.Length < 5)
                    {
                        P_INVOICE_DOCS invoiceDoc = new P_INVOICE_DOCS();
                        byte[] avatar = new byte[Request.Files["docUpload"].InputStream.Length];
                        Request.Files["docUpload"].InputStream.Read(avatar, 0, avatar.Length);
                        invoiceDoc.InvoiceID = invoice.ID;
                        invoiceDoc.Doc = avatar;

                        db.P_INVOICE_DOCS.Add(invoiceDoc);
                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Faylni yuklab bo'lmadi, u 2MBdan kattaroq. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
                        throw new RetryLimitExceededException();
                    }
                }

                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "PInvoiceController", "Create[Post]");

                // Redirect to PackingList create view with necessary Invoice properties
                return RedirectToAction("Create", "PackingList", new { invoiceId = newInvoiceID, invoiceNo = invoice.InvoiceNo });
            }
        }

        private void PopulateViewBags()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                //ViewBag.POrder = new SelectList(db.P_ORDERS.Where(x => x.IsDeleted == false).ToList(), "ID", "OrderNo");
                ViewBag.POrder = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
            }
        }
        public ActionResult Download(int invoiceID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var invoiceDoc = db.P_INVOICE_DOCS.FirstOrDefault(pi => pi.InvoiceID == invoiceID);
                if (invoiceDoc != null)
                    return File(invoiceDoc.Doc, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                else
                    return HttpNotFound("Fayl topilmadi.");
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            P_INVOICES invoice;
            List<P_INVOICE_PARTS> partList;
            string transportNo = null;
            string packingListNo = null;
            List<P_INVOICE_PACKINGLISTS> packingLists;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                invoice = db.P_INVOICES
                    .Include(i => i.COMPANy)
                    .Include(i => i.SUPPLIER)
                    .Include(i => i.P_ORDERS.P_CONTRACTS)
                    .Include(i => i.P_INVOICE_PACKINGLISTS.Select(p => p.F_TRANSPORT_TYPES))
                    .Where(i => i.ID == id).FirstOrDefault();

                if (invoice == null)
                    return HttpNotFound();

                partList = db.P_INVOICE_PARTS
                    .Include(ip => ip.PART)
                    .Include(ip => ip.UNIT)
                    .Where(ip => ip.InvoiceID == invoice.ID)
                    .ToList();

                packingLists = db.P_INVOICE_PACKINGLISTS
                    .Include(p => p.F_TRANSPORT_TYPES)
                    .Include(p => p.P_PACKINGLIST_PARTS)
                    .Where(p => p.InvoiceID == invoice.ID)
                    .ToList();

                List<P_PACKINGLIST_PARTS> packingListParts = db.P_PACKINGLIST_PARTS.Include(p => p.P_INVOICE_PACKINGLISTS).Include(p => p.PART).ToList();

                var firstPackingList = invoice.P_INVOICE_PACKINGLISTS.FirstOrDefault();
                if (firstPackingList != null)
                {
                    transportNo = firstPackingList.TransportNo;
                    packingListNo = firstPackingList.PackingListNo;
                }

                foreach (var part in partList)
                {
                    db.Entry(part).Reference(p => p.PART).Load();
                }

                ViewBag.Invoice = invoice;
                ViewBag.PartList = partList;
                ViewBag.PackingLists = packingLists;
                ViewBag.PackingListParts = packingListParts;
            }

            ViewBag.packingListNo = packingListNo;
            ViewBag.transportNo = transportNo;
            ViewBag.partList = partList;

            return View(invoice);
        }
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var invoice = db.P_INVOICES.Find(Id);
                if (invoice == null)
                {
                    return HttpNotFound();
                }
                else
                    ViewBag.partList = db.P_INVOICE_PARTS
                        .Include(pc => pc.PART)
                        .Include(pc => pc.UNIT)
                        .Where(pc => pc.InvoiceID == invoice.ID).ToList();

                db.Entry(invoice).Reference(i => i.P_ORDERS).Load();
                db.Entry(invoice).Reference(i => i.SUPPLIER).Load();

                return View(invoice);
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
                    P_INVOICES invoiceToDelete = db.P_INVOICES.Find(ID);
                    if (invoiceToDelete != null)
                    {
                        invoiceToDelete.IsDeleted = true;
                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PInvoiceController", "Delete[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bunday invoice topilmadi.");
                    }
                }
            }
            return View();
        }
        public ActionResult DeletePart(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var invoicePartToDelete = db.P_INVOICE_PARTS.Find(id);
                if (ModelState.IsValid)
                {
                    if (invoicePartToDelete != null)
                    {
                        try
                        {
                            db.P_INVOICE_PARTS.Remove(invoicePartToDelete);
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PInvoiceController", "DeletePart[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "InvoicePart not found.");
                    }
                }
                return View(invoicePartToDelete);
            }
        }
        public ActionResult Edit(int? ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                // Eager loading P_INVOICE_PARTS and their associated PARTs
                var invoice = db.P_INVOICES
                                .Include(i => i.P_INVOICE_PARTS.Select(pc => pc.PART))
                                .Include(i => i.P_ORDERS)

                                .FirstOrDefault(i => i.ID == ID);

                if (invoice == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", invoice.SupplierID);
                ViewBag.POrder = new SelectList(db.P_ORDERS.Where(x => x.IsDeleted == false).ToList(), "ID", "OrderNo", invoice.OrderID);
                ViewBag.partList = db.P_INVOICE_PARTS.Include(x => x.UNIT).Where(x => x.InvoiceID == ID).ToList();

                return View(invoice);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(P_INVOICES invoice)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    P_INVOICES invoiceToUpdate = db.P_INVOICES.Find(invoice.ID);
                    if (invoiceToUpdate != null)
                    {
                        invoiceToUpdate.InvoiceNo = invoice.InvoiceNo;
                        invoiceToUpdate.SupplierID = invoice.SupplierID;
                        invoiceToUpdate.OrderID = invoice.OrderID;
                        invoiceToUpdate.InvoiceDate = invoice.InvoiceDate;
                        invoiceToUpdate.Currency = invoice.Currency;
                        invoiceToUpdate.IsDeleted = false;

                        db.Entry(invoiceToUpdate).State = EntityState.Modified;

                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PInvoiceController", "Edit[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException?.InnerException is SqlException sqlEx)
                            {
                                if (sqlEx.Number == 547) // Foreign key constraint violation
                                {
                                    ModelState.AddModelError("", "The OrderID does not exist in the P_ORDERS table.");
                                }
                                else
                                {
                                    ModelState.AddModelError("", $"Database update error: {sqlEx.Message}");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                            }
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact the system administrator.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invoice not found.");
                    }
                }

                // Re-populate dropdown lists in case of an error
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", invoice.SupplierID);
                ViewBag.POrder = new SelectList(db.P_ORDERS.Where(x => x.IsDeleted == false).ToList(), "ID", "OrderNo", invoice.OrderID);
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                ViewBag.partList = db.P_INVOICE_PARTS
                    .Include(pc => pc.PART)
                    .Include(pc => pc.UNIT)
                    .Where(pc => pc.InvoiceID == invoice.ID).ToList();

                return View(invoice);
            }
        }
        public ActionResult EditPart(int? ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var invoicePart = db.P_INVOICE_PARTS
                    .Include(ip => ip.P_INVOICES)
                    .Include(ip => ip.UNIT)
                    .SingleOrDefault(pi => pi.ID == ID);
                if (invoicePart == null)
                {
                    return HttpNotFound();
                }
                var allParts = db.PARTS.Select(p => new SelectListItem
                {
                    Value = p.ID.ToString(),
                    Text = p.PNo
                }).ToList();


                ViewBag.PartList = allParts;

                return View(invoicePart);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPart(P_INVOICE_PARTS invoicePart)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    P_INVOICE_PARTS invoicePartToUpdate = db.P_INVOICE_PARTS.Find(invoicePart.ID);
                    if (invoicePartToUpdate != null)
                    {
                        invoicePartToUpdate.PartID = invoicePart.PartID;
                        invoicePartToUpdate.Price = invoicePart.Price;
                        invoicePartToUpdate.Quantity = invoicePart.Quantity;
                        invoicePartToUpdate.UnitID = invoicePart.UnitID;
                        //invoicePartToUpdate.Amount = orderPart.Quantity * orderPart.Price; SQL o'zi chiqarib beradi

                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "PInvoiceController", "EditPart[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    return View(invoicePartToUpdate);
                }
                return View();
            }
        }

        public async Task<ActionResult> Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES invoys = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("invoys.xlsx") == 0).FirstOrDefault();
                if (invoys != null)
                    return File(invoys.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

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
                                orderNo = row["OrderNo"].ToString();
                                supplierName = row["Supplier Name"].ToString();
                                partNo = row["Part Number"].ToString();
                                invoiceNo = row["Invoice No."].ToString();

                                SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                                PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                P_ORDERS order = db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.IsDeleted == false).FirstOrDefault();
                                P_INVOICES invoice = db.P_INVOICES.Where(pi => pi.InvoiceNo.CompareTo(invoiceNo) == 0 && pi.SupplierID == supplier.ID && pi.OrderID == order.ID && pi.IsDeleted == false).FirstOrDefault();

                                if (invoice != null)
                                {
                                    P_INVOICE_PARTS invoicePart = db.P_INVOICE_PARTS.Where(pop => pop.PartID == part.ID && pop.InvoiceID == invoice.ID).FirstOrDefault();
                                    if (invoicePart != null)
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

                var tableModel = JsonConvert.DeserializeObject<DataTable>(dataTableModel);

                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        foreach (DataRow row in tableModel.Rows)
                        {
                            orderNo = row["OrderNo"].ToString();
                            supplierName = row["Supplier Name"].ToString();
                            partNo = row["Part Number"].ToString();
                            invoiceNo = row["Invoice No."].ToString();

                            SUPPLIER supplier = await db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefaultAsync();
                            PART part = await db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefaultAsync();
                            P_ORDERS order = await db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.IsDeleted == false).FirstOrDefaultAsync();
                            P_INVOICES invoice = await db.P_INVOICES.Where(pi => pi.InvoiceNo.CompareTo(invoiceNo) == 0 && pi.SupplierID == supplier.ID && pi.OrderID == order.ID && pi.IsDeleted == false).FirstOrDefaultAsync();

                            if (invoice == null)
                            {
                                P_INVOICES new_invoice = new P_INVOICES();
                                new_invoice.InvoiceNo = invoiceNo;
                                new_invoice.OrderID = order.ID;
                                new_invoice.SupplierID = supplier.ID;
                                new_invoice.InvoiceDate = DateTime.Parse(row["Date"].ToString());
                                new_invoice.Currency = row["Currency"].ToString();
                                new_invoice.CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["companyID"]);
                                new_invoice.IsDeleted = false;

                                db.P_INVOICES.Add(new_invoice);
                                await db.SaveChangesAsync();

                                P_INVOICE_PARTS invoicePart = await db.P_INVOICE_PARTS.Where(pcp => pcp.InvoiceID == new_invoice.ID && pcp.PartID == part.ID).FirstOrDefaultAsync();
                                if (invoicePart == null)
                                {
                                    P_INVOICE_PARTS new_invoicePart = new P_INVOICE_PARTS();
                                    new_invoicePart.PartID = part.ID;
                                    new_invoicePart.InvoiceID = new_invoice.ID;
                                    new_invoicePart.Price = Convert.ToDouble(row["Price"].ToString());
                                    //new_invoicePart.Unit = row["Unit"].ToString();
                                    new_invoicePart.Quantity = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_INVOICE_PARTS.Add(new_invoicePart);
                                    await db.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                P_INVOICE_PARTS invoicePart = await db.P_INVOICE_PARTS.Where(pcp => pcp.InvoiceID == invoice.ID && pcp.PartID == part.ID).FirstOrDefaultAsync();
                                if (invoicePart == null)
                                {
                                    P_INVOICE_PARTS new_invoicePart = new P_INVOICE_PARTS();
                                    new_invoicePart.PartID = part.ID;
                                    new_invoicePart.InvoiceID = invoice.ID;
                                    new_invoicePart.Price = Convert.ToDouble(row["Price"].ToString());
                                    //new_invoicePart.Unit = row["Unit"].ToString();
                                    new_invoicePart.Quantity = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_INVOICE_PARTS.Add(new_invoicePart);
                                    await db.SaveChangesAsync();
                                }
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
            LogHelper.LogToDatabase(userEmail, "PInvoiceController", "Save[Post]");
            return RedirectToAction("Index");
        }
    }
}