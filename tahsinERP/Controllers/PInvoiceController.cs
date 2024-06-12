using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PInvoiceController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private readonly string[] sources = new string[4] { "", "KD", "Steel", "Maxalliy" };
        private string supplierName, invoiceNo, orderNo, partNo = "";
        // GET: PInvoice
        public ActionResult Index(string type, int? supplierID)
        {
            if (!string.IsNullOrEmpty(type))
            {
                if (supplierID.HasValue)
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false && pi.SUPPLIER.Type.CompareTo(type) == 0 && pi.SupplierID == supplierID).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name", supplierID);

                    return View(list);
                }
                else
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false && pi.SUPPLIER.Type.CompareTo(type) == 0).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name");

                    return View(list);
                }
            }
            else
            {
                if (supplierID.HasValue)
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false && pi.SupplierID == supplierID).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", supplierID);

                    return View(list);
                }
                else
                {
                    List<P_INVOICES> list = db.P_INVOICES.Where(pi => pi.IsDeleted == false).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name");

                    return View(list);
                }
            }
        }
        public ActionResult Create()
        {
            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name");
            ViewBag.POrder = new SelectList(db.P_ORDERS, "ID", "ContractNo");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InvoiceNo, CompanyID, SupplierID, OrderID, InvoiceDate, Amount, Currency")] P_INVOICES invoice)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    invoice.IsDeleted = false;
                    db.P_INVOICES.Add(invoice);

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }

            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name", invoice.SupplierID);
            ViewBag.POrder = new SelectList(db.P_CONTRACTS, "ID", "ContractNo", invoice.OrderID);

            return View(invoice);
        }
        public async Task<ActionResult> Download()
        {
            SAMPLE_FILES invoys = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("invoys.xlsx") == 0).FirstOrDefault();
            if (invoys != null)
                return File(invoys.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return View();
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return RedirectToAction("Index");
        }
    }
}