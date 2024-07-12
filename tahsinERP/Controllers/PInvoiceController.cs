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
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
        private string supplierName, invoiceNo, orderNo, partNo = "";
        //private DBTHSNEntities db = new DBTHSNEntities();
        // GET: PInvoice
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                IQueryable<P_INVOICES> invoicesQuery = db.P_INVOICES
                    .Include(pi => pi.SUPPLIER)
                    .Include(pi => pi.P_ORDERS)
                    .Where(pi => pi.IsDeleted == false);

                // Filter by type if provided
                if (!string.IsNullOrEmpty(type))
                {
                    invoicesQuery = invoicesQuery.Where(pi => pi.SUPPLIER.Type.CompareTo(type) == 0);
                    ViewBag.SourceList = new SelectList(sources, type);
                }
                else
                {
                    ViewBag.SourceList = new SelectList(sources);
                }

                // Filter by supplierID if provided
                if (supplierID.HasValue)
                {
                    invoicesQuery = invoicesQuery.Where(pi => pi.SupplierID == supplierID.Value);
                }

                List<P_INVOICES> invoices = invoicesQuery.ToList();

                // Prepare ViewBag.SupplierList based on filters
                var suppliersQuery = db.SUPPLIERS.Where(s => s.IsDeleted == false);

                if (!string.IsNullOrEmpty(type))
                {
                    suppliersQuery = suppliersQuery.Where(s => s.Type.CompareTo(type) == 0);
                }

                if (supplierID.HasValue)
                {
                    suppliersQuery = suppliersQuery.Where(s => s.ID == supplierID.Value);
                }

                ViewBag.SupplierList = new SelectList(suppliersQuery.Where(s => s.Type.CompareTo(type) == 0).ToList(), "ID", "Name");
                ViewBag.Type = type;

                return View(invoices);
            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name");
                ViewBag.POrder = new SelectList(db.P_ORDERS.ToList(), "ID", "OrderNo");
                ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PInvoiceViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                P_INVOICES invoice = new P_INVOICES()
                {
                    InvoiceNo = model.InvoiceNo,
                    OrderID = model.OrderID,
                    SupplierID = model.SupplierID,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    InvoiceDate = model.InvoiceDate,
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
                        Unit = item.Unit,
                        Price = item.Price
                    };
                    db.P_INVOICE_PARTS.Add(newPart);
                }

                ViewBag.POrder = new SelectList(db.P_ORDERS.Where(s => s.IsDeleted == false).ToList(), "ID", "OrderNo", invoice.OrderID);
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", invoice.SupplierID);
                ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");

                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            P_INVOICES invoice;
            List<P_INVOICE_PARTS> partList;
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                invoice = db.P_INVOICES.Find(id);

                if (invoice == null)
                    return HttpNotFound();

                // Manually load the related entities
                db.Entry(invoice).Reference(i => i.P_ORDERS).Load();
                db.Entry(invoice).Reference(i => i.SUPPLIER).Load();

                partList = db.P_INVOICE_PARTS
                                        .Include(ip => ip.PART)
                                        .Where(ip => ip.InvoiceID == invoice.ID).ToList();
                foreach (var part in partList)
                {
                    db.Entry(part).Reference(p => p.PART).Load();
                }
            }

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
        /*public ActionResult Edit(int? ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var invoice = db.P_INVOICES.Find(ID);
                if (invoice == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Supplier = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name", invoice.SupplierID);
                ViewBag.POrder = new SelectList(db.P_ORDERS.ToList(), "ID", "OrderNo", invoice.OrderID);
                ViewBag.partList = db.P_INVOICE_PARTS.Where(pc => pc.InvoiceID == invoice.ID).ToList();

                return View(invoice);
            }
        }*/
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
                                .SingleOrDefault(i => i.ID == ID);

                if (invoice == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Supplier = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name", invoice.SupplierID);
                ViewBag.POrder = new SelectList(db.P_ORDERS.ToList(), "ID", "OrderNo", invoice.OrderID);
                ViewBag.partList = invoice.P_INVOICE_PARTS.ToList();

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
                        invoiceToUpdate.Amount = invoice.Amount;
                        invoiceToUpdate.IsDeleted = false;

                        db.Entry(invoiceToUpdate).State = EntityState.Modified;

                        try
                        {
                            db.SaveChanges();
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
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name", invoice.SupplierID);
                ViewBag.POrder = new SelectList(db.P_ORDERS.ToList(), "ID", "OrderNo", invoice.OrderID);
                ViewBag.partList = db.P_INVOICE_PARTS
                    .Include(pc => pc.PART)
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

                var invoicePart = db.P_INVOICE_PARTS.Include(ip => ip.P_INVOICES).SingleOrDefault(pi => pi.ID == ID);
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
                        invoicePartToUpdate.Unit = invoicePart.Unit;
                        //invoicePartToUpdate.Amount = orderPart.Quantity * orderPart.Price; SQL o'zi chiqarib beradi


                        if (TryUpdateModel(invoicePartToUpdate, "", new string[] { "PartID, Price, Quantity, Unit" }))
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
            await Task.Run(() =>
            {
                // Perform CPU-bound work here
                // For example, heavy computations or other synchronous tasks

            });
            if (!string.IsNullOrEmpty(dataTableModel))
            {

                var tableModel = JsonConvert.DeserializeObject<System.Data.DataTable>(dataTableModel);

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

                            SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                            P_ORDERS order = db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.IsDeleted == false).FirstOrDefault();
                            P_INVOICES invoice = db.P_INVOICES.Where(pi => pi.InvoiceNo.CompareTo(invoiceNo) == 0 && pi.SupplierID == supplier.ID && pi.OrderID == order.ID && pi.IsDeleted == false).FirstOrDefault();

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
                                db.SaveChanges();

                                P_INVOICE_PARTS invoicePart = db.P_INVOICE_PARTS.Where(pcp => pcp.InvoiceID == new_invoice.ID && pcp.PartID == part.ID).FirstOrDefault();
                                if (invoicePart == null)
                                {
                                    P_INVOICE_PARTS new_invoicePart = new P_INVOICE_PARTS();
                                    new_invoicePart.PartID = part.ID;
                                    new_invoicePart.InvoiceID = new_invoice.ID;
                                    new_invoicePart.Price = Convert.ToDouble(row["Price"].ToString());
                                    new_invoicePart.Unit = row["Unit"].ToString();
                                    new_invoicePart.Quantity = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_INVOICE_PARTS.Add(new_invoicePart);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                P_INVOICE_PARTS invoicePart = db.P_INVOICE_PARTS.Where(pcp => pcp.InvoiceID == invoice.ID && pcp.PartID == part.ID).FirstOrDefault();
                                if (invoicePart == null)
                                {
                                    P_INVOICE_PARTS new_invoicePart = new P_INVOICE_PARTS();
                                    new_invoicePart.PartID = part.ID;
                                    new_invoicePart.InvoiceID = invoice.ID;
                                    new_invoicePart.Price = Convert.ToDouble(row["Price"].ToString());
                                    new_invoicePart.Unit = row["Unit"].ToString();
                                    new_invoicePart.Quantity = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_INVOICE_PARTS.Add(new_invoicePart);
                                    db.SaveChanges();
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
            return RedirectToAction("Index");
        }
    }
}