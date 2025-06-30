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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels.PInvoice;

namespace tahsinERP.Controllers
{
    public class PInvoiceController : Controller
    {
        private string supplierName, invoiceNo, orderNo, transportNo, transportType, partNo = "";
        private string[] sources;

        private List<string> missingOrders = new List<string>();
        private List<string> missingSuppliers = new List<string>();
        private List<string> missingParts = new List<string>();

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
        public ActionResult GetPartList(int orderID)
        {
            using (var db = new DBTHSNEntities())
            {
                var partList = db.P_ORDER_PARTS
                    .Where(cp => cp.OrderID == orderID)
                    .Select(cp => new
                    {
                        cp.PartID,
                        cp.PART.PNo
                    })
                    .ToList();

                return Json(new { success = true, data = partList }, JsonRequestBehavior.AllowGet);
            }
        }
        //Invoice currency ni Orderdan olish
        public JsonResult GetOrderDetails(int orderID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var order = db.P_ORDERS
                                 .Where(c => c.ID == orderID && c.IsDeleted == false)
                                 .Select(c => new
                                 {
                                     c.ID,
                                     c.Currency
                                 })
                                 .FirstOrDefault();

                if (order != null)
                {
                    return Json(new { success = true, data = order }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Contract not found." }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult GetPriceAndQuantity(int partID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var orderPart = db.P_ORDER_PARTS
                                     .Where(cp => cp.PartID == partID)
                                     .Select(cp => new
                                     {
                                         Price = cp.Price,
                                         Quantity = cp.Amount
                                     })
                                     .FirstOrDefault();

                if (orderPart != null)
                {
                    return Json(new { success = true, data = orderPart }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Part not found in contract." }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Create()
        {
            PopulateViewBags();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PInvoiceCreateViewModel model)
        {
            PopulateViewBags();
            try
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

                    LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{invoice.InvoiceNo} - PInvoiceni yaratdi");

                    List<P_ORDER_PARTS> orderParts = db.P_ORDER_PARTS.Where(po => po.OrderID == model.OrderID).ToList();
                    List<string> notInOrderParts = new List<string>();

                    foreach (var item in model.Parts)
                    {
                        var orderPart = orderParts.Where(p => p.PartID == item.PartID).FirstOrDefault();
                        if (orderParts.Where(cp => cp.PartID == item.PartID).Any())
                        {
                            var newPart = new P_INVOICE_PARTS
                            {
                                InvoiceID = invoice.ID,
                                PartID = item.PartID,
                                Quantity = item.Quantity,
                                UnitID = item.UnitID
                            };

                            newPart.Price = orderPart.Price;
                            db.P_INVOICE_PARTS.Add(newPart);
                            var logPart = db.PARTS.Find(item.PartID);
                            LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{logPart.PNo} - PInvoicenPartni yaratdi");
                        }
                        else
                        {
                            PART paart = db.PARTS.Where(p => p.ID == item.PartID).FirstOrDefault();
                            notInOrderParts.Add(paart.PNo);
                        }
                    }

                    if (notInOrderParts.Count > 0)
                    {
                        var message = "";
                        foreach (var word in notInOrderParts)
                        {
                            message += word + " ,";
                        }
                        ModelState.AddModelError("", "Ushbu ehtiyot qism(lar): " + message + " buyurtmadan topilmadi, buyurtmada yo'q narsaga invoys qilib bo'lmaydi! Qaytadan urinib ko'ring!");
                        db.Entry(invoice).State = EntityState.Deleted;
                        db.SaveChanges();
                        return View(model);
                    }

                    db.SaveChanges();

                    if (model.File != null)
                    {
                        if (Request.Files["docUpload"].ContentLength > 0)
                        {
                            if (Request.Files["docUpload"].InputStream.Length < 5242880)
                            {
                                if (Path.GetExtension(model.File.FileName).ToLower() == ".pdf")
                                {
                                    P_INVOICE_DOCS invoiceDoc = new P_INVOICE_DOCS();
                                    byte[] avatar = new byte[Request.Files["docUpload"].InputStream.Length];
                                    Request.Files["docUpload"].InputStream.Read(avatar, 0, avatar.Length);
                                    invoiceDoc.InvoiceID = invoice.ID;
                                    invoiceDoc.Doc = avatar;

                                    db.P_INVOICE_DOCS.Add(invoiceDoc);
                                    db.SaveChanges();
                                    LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{invoiceDoc.P_INVOICES.InvoiceNo} - uchun PInvoiceDocni yaratdi");
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Format noto'g'ri. Faqat .pdf fayllarni yuklash mumkin.");
                                }

                            }
                            else
                            {
                                ModelState.AddModelError("", "Faylni yuklab bo'lmadi, u 2MBdan kattaroq. Qayta urinib ko'ring, agar muammo yana qaytarilsa, tizim administratoriga murojaat qiling.");
                                throw new RetryLimitExceededException();
                            }
                        }
                    }

                    // Redirect to PackingList create view with necessary Invoice properties
                    return RedirectToAction("Create", "PackingList", new { invoiceId = invoice.ID, invoiceNo = invoice.InvoiceNo });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }
        private void PopulateViewBags()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                //ViewBag.POrder = new SelectList(db.P_ORDERS.Where(x => x.IsDeleted == false).ToList(), "ID", "OrderNo");
                ViewBag.POrder = new SelectList(Enumerable.Empty<SelectListItem>());
                //ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
                ViewBag.partList = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
            }
        }
        public ActionResult DownloadDoc(int? invoiceID)
        {
            if (invoiceID == null)
            {
                return Json(new { success = false, message = "Invoice ID ko'rsatilmagan."}, JsonRequestBehavior.AllowGet);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var invoiceDoc = db.P_INVOICE_DOCS
                                        .Include(x => x.P_INVOICES)
                                        .FirstOrDefault(pi => pi.InvoiceID == invoiceID);
                if (invoiceDoc != null)
                {
                    string invoiceNo = invoiceDoc.P_INVOICES.InvoiceNo;
                    return File(invoiceDoc.Doc, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", invoiceNo + "_InvoiceDoc.pdf");
                }
                else
                {
                    return Json(new { success = false, message = "ushbu invoys uchun dokument yuklanmagan" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Details(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

                P_INVOICES invoice;
                List<P_INVOICE_PARTS> partList;
                string transportNo = null;
                string packingListNo = null;
                List<P_INVOICE_PACKINGLISTS> packingLists;

                invoice = db.P_INVOICES
                    .Include(i => i.COMPANy)
                    .Include(i => i.SUPPLIER)
                    .Include(i => i.P_ORDERS.P_CONTRACTS)
                    .Include(i => i.P_INVOICE_PACKINGLISTS.Select(p => p.F_TRANSPORT_TYPES))
                    .Where(i => i.ID == id).FirstOrDefault();

                if (invoice == null)
                    return HttpNotFound();
                else
                {
                    partList = db.P_INVOICE_PARTS
                        .Include(ip => ip.PART)
                        .Include(ip => ip.UNIT)
                        .Where(ip => ip.InvoiceID == invoice.ID)
                        .ToList();

                    packingLists = invoice.P_INVOICE_PACKINGLISTS.ToList();

                    List<P_PACKINGLIST_PARTS> packingListParts = new List<P_PACKINGLIST_PARTS>();
                    List<P_PACKINGLIST_PARTS> VP = new List<P_PACKINGLIST_PARTS>();

                    for (int i = 0; i < packingLists.Count; i++)
                    {
                        VP = packingLists[i].P_PACKINGLIST_PARTS.ToList();
                        for (int j = 0; j < VP.Count; j++)
                        {
                            packingListParts.Add(VP[j]);
                        }
                    }

                    ViewBag.Invoice = invoice;
                    ViewBag.PartList = partList;
                    ViewBag.PackingLists = packingLists;
                    ViewBag.PackingListParts = packingListParts;
                }
                var firstPackingList = invoice.P_INVOICE_PACKINGLISTS.FirstOrDefault();
                if (firstPackingList != null)
                {
                    transportNo = firstPackingList.TransportNo;
                    packingListNo = firstPackingList.PackingListNo;
                }

                ViewBag.packingListNo = packingListNo;
                ViewBag.transportNo = transportNo;
                ViewBag.partList = partList;

                return View(invoice);
            }
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

                            LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{invoiceToDelete.InvoiceNo} - PInvoiceni o'chirdi");

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

                            LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{id} ID ga ega PInvoicePartni o'chirdi");

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
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
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

                            LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{invoice.InvoiceNo} - PInvoiceni tahrirladi");

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


                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
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

                            LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{invoicePart.PART.PNo} - InvoicePartni tahrirladi");

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
                SAMPLE_FILES invoys = await db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("invoys.xlsx") == 0).FirstOrDefaultAsync();
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
        private async Task<bool> CheckForExistenceOfOrders(DataTable dataTable)
        {
            if (dataTable == null)
                return false;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    string orderNo = row["OrderNo"].ToString();
                    if (!orderNo.IsNullOrWhiteSpace())
                    {

                        bool orderExists = await db.P_ORDERS.AnyAsync(p => p.OrderNo.CompareTo(orderNo) == 0);
                        if (!orderExists)
                        {
                            if (!missingOrders.Contains(orderNo))
                            {
                                missingOrders.Add(orderNo);
                            }
                            return false;  // Return false immediately if any contract is missing
                        }
                    }
                }
            }
            return true;  // Return true if all contracts exist
        }
        private async Task<bool> CheckForExistenceOfParts(DataTable dataTable)
        {
            if (dataTable == null)
                return false;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    string partPNo = row["Part Number"].ToString();

                    bool partExists = await db.PARTS.AnyAsync(p => p.PNo.CompareTo(partPNo) == 0);
                    if (!partExists)
                    {
                        if (!missingParts.Contains(partPNo))
                        {
                            missingParts.Add(partPNo);
                        }
                        return false;  // Return false immediately if any part is missing
                    }
                }
            }

            return true;  // Return true if all parts exist
        }
        private async Task<bool> CheckForExistenceOfSuppliers(DataTable dataTable)
        {
            if (dataTable == null)
                return false;
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    string supplierName = row["Supplier Name"].ToString();

                    bool supplierExists = await db.SUPPLIERS.AnyAsync(s => s.Name.CompareTo(supplierName) == 0);
                    if (!supplierExists)
                    {
                        if (!missingSuppliers.Contains(supplierName))
                        {
                            missingSuppliers.Add(supplierName);
                        }
                        return false;  // Return false immediately if any supplier is missing
                    }
                }
            }
            return true;  // Return true if all suppliers exist
        }
        [HttpPost]
        public async Task<ActionResult> UploadWithExcel(HttpPostedFileBase file)
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
                        if (await CheckForExistenceOfOrders(dataTable))
                        {
                            if (await CheckForExistenceOfSuppliers(dataTable))
                            {
                                if (await CheckForExistenceOfParts(dataTable))
                                {
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
                                else
                                {
                                    var message = "";
                                    foreach (var word in missingParts)
                                    {
                                        message += word + ", ";
                                    }
                                    ViewBag.Message = "Ushbu fakturalar faylida kiritilgan ehtiyot qismlar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Fakturalar bazasiga kiritib keyin qayta urining.";
                                    return View("UploadWithExcel");
                                }
                            }
                            else
                            {
                                var message = "";
                                foreach (var word in missingSuppliers)
                                {
                                    message += word + ", ";
                                }
                                ViewBag.Message = "Ushbu fakturalar faylda kiritilgan ta'minotchilar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Fakturalar kiritib keyin qayta urining.";
                                return View("UploadWithExcel");
                            }
                        }
                        else
                        {
                            var message = "";
                            foreach (var word in missingOrders)
                            {
                                message += word + ", ";
                            }
                            ViewBag.Message = "Ushbu faktura faylda kiritilgan buyurtmalar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Buyurtmalar bazasiga kiritib keyin qayta urining.";
                            return View("UploadWithExcel");
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
                            transportNo = row["Transport No."].ToString();
                            string packinglistNo = invoiceNo + "_PL";
                            transportType = row["Transport Type"].ToString();

                            SUPPLIER supplier = await db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefaultAsync();
                            PART part = await db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefaultAsync();
                            P_ORDERS order = await db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.IsDeleted == false).FirstOrDefaultAsync();
                            P_INVOICES invoice = await db.P_INVOICES.Where(pi => pi.InvoiceNo.CompareTo(invoiceNo) == 0 && pi.SupplierID == supplier.ID && pi.OrderID == order.ID && pi.IsDeleted == false).FirstOrDefaultAsync();
                            string unitName = row["Unit"].ToString();

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

                                P_INVOICE_PACKINGLISTS newPackingList = new P_INVOICE_PACKINGLISTS();
                                newPackingList.PackingListNo = packinglistNo;
                                newPackingList.TransportNo = transportNo;
                                newPackingList.InvoiceID = new_invoice.ID;
                                newPackingList.SealNo = "";
                                F_TRANSPORT_TYPES transport = db.F_TRANSPORT_TYPES.Where(t => t.TransportType.CompareTo(transportType) == 0).FirstOrDefault();
                                if (transport != null)
                                    newPackingList.TransportTypeID = transport.ID;
                                else
                                    newPackingList.TransportTypeID = 1;
                                newPackingList.InTransit = true;
                                newPackingList.IsDeleted = false;
                                db.P_INVOICE_PACKINGLISTS.Add(newPackingList);
                                await db.SaveChangesAsync();

                                LogHelper.LogToDatabase(User.Identity.Name, "PInvoiceController", $"{new_invoice.InvoiceNo} - invoiceni Excel orqali yaratdi");
                                LogHelper.LogToDatabase(User.Identity.Name, "PackinglistController", $"{newPackingList.PackingListNo} - packinglistni Excel orqali kiritdi");

                                P_INVOICE_PARTS invoicePart = await db.P_INVOICE_PARTS.Where(pcp => pcp.InvoiceID == new_invoice.ID && pcp.PartID == part.ID).FirstOrDefaultAsync();
                                if (invoicePart == null)
                                {
                                    P_INVOICE_PARTS new_invoicePart = new P_INVOICE_PARTS();
                                    new_invoicePart.PartID = part.ID;
                                    new_invoicePart.InvoiceID = new_invoice.ID;
                                    new_invoicePart.Price = Convert.ToDouble(row["Price"].ToString());
                                    UNIT unit = db.UNITS.Where(x => x.ShortName == unitName).FirstOrDefault();
                                    if (unit is null)
                                        new_invoicePart.UnitID = 1;
                                    else
                                        new_invoicePart.UnitID = unit.ID;

                                    new_invoicePart.Quantity = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_INVOICE_PARTS.Add(new_invoicePart);
                                    await db.SaveChangesAsync();

                                    P_PACKINGLIST_PARTS newPackingListPart = new P_PACKINGLIST_PARTS();

                                    newPackingListPart.PackingListID = newPackingList.ID;
                                    newPackingListPart.PartID = part.ID;
                                    newPackingListPart.PrLength = Convert.ToDouble(row["Length"].ToString());
                                    newPackingListPart.PrWidth = Convert.ToDouble(row["Width"].ToString());
                                    newPackingListPart.PrHeight = Convert.ToDouble(row["Height"].ToString());
                                    newPackingListPart.PrAmount = Convert.ToDouble(row["Box Amount"].ToString());
                                    newPackingListPart.PieceWeight = Convert.ToDouble(row["Piece weight"].ToString());
                                    newPackingListPart.PrNetWeight = Convert.ToDouble(row["NetWeight"].ToString());
                                    newPackingListPart.PrGrWeight = Convert.ToDouble(row["GrWeight"].ToString());
                                    newPackingListPart.TotalPrPacks = Convert.ToInt32(row["Box quantity"].ToString());
                                    newPackingListPart.TotalNetWeight = Convert.ToDouble(row["TotalNetWeight"].ToString());
                                    newPackingListPart.TotalGrWeight = Convert.ToDouble(row["TotalGrWeight"].ToString());

                                    db.P_PACKINGLIST_PARTS.Add(newPackingListPart);
                                    await db.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                P_INVOICE_PARTS invoicePart = await db.P_INVOICE_PARTS.Where(pcp => pcp.InvoiceID == invoice.ID && pcp.PartID == part.ID).FirstOrDefaultAsync();
                                P_INVOICE_PACKINGLISTS packingList = db.P_INVOICE_PACKINGLISTS.Where(pl => pl.InvoiceID == invoice.ID).FirstOrDefault();
                                if (invoicePart == null)
                                {
                                    P_INVOICE_PARTS new_invoicePart = new P_INVOICE_PARTS();
                                    new_invoicePart.PartID = part.ID;
                                    new_invoicePart.InvoiceID = invoice.ID;
                                    new_invoicePart.Price = Convert.ToDouble(row["Price"].ToString());
                                    UNIT unit = db.UNITS.Where(x => x.ShortName == unitName).FirstOrDefault();
                                    if (unit is null)
                                        new_invoicePart.UnitID = 1;
                                    else
                                        new_invoicePart.UnitID = unit.ID;

                                    new_invoicePart.Quantity = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_INVOICE_PARTS.Add(new_invoicePart);
                                    await db.SaveChangesAsync();

                                    if (packingList != null)
                                    {
                                        P_PACKINGLIST_PARTS newPackingListPart = new P_PACKINGLIST_PARTS();

                                        newPackingListPart.PackingListID = packingList.ID;
                                        newPackingListPart.PartID = part.ID;
                                        newPackingListPart.PrLength = Convert.ToDouble(row["Length"].ToString());
                                        newPackingListPart.PrWidth = Convert.ToDouble(row["Width"].ToString());
                                        newPackingListPart.PrHeight = Convert.ToDouble(row["Height"].ToString());
                                        newPackingListPart.PrAmount = Convert.ToDouble(row["Box Amount"].ToString());
                                        newPackingListPart.PieceWeight = Convert.ToDouble(row["Piece weight"].ToString());
                                        newPackingListPart.PrNetWeight = Convert.ToDouble(row["NetWeight"].ToString());
                                        newPackingListPart.PrGrWeight = Convert.ToDouble(row["GrWeight"].ToString());
                                        newPackingListPart.TotalPrPacks = Convert.ToInt32(row["Box Quantity"].ToString());
                                        newPackingListPart.TotalNetWeight = Convert.ToDouble(row["TotalNetWeight"].ToString());
                                        newPackingListPart.TotalGrWeight = Convert.ToDouble(row["TotalGrWeight"].ToString());

                                        db.P_PACKINGLIST_PARTS.Add(newPackingListPart);
                                        await db.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }

                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "PInvoiceController", "Save[Post]");
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