using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class WhIncomeController : Controller
    {
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
        private string partNo, waybillNo, whName, docNo, invoiceNo = "";
        // GET: WhIncome
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (!string.IsNullOrEmpty(type))
                {
                    if (supplierID.HasValue)
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0 && pi.P_INVOICES.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
            }
        }
        public ActionResult Create()
        {
            return View();
        }
        public async Task<ActionResult> Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES invoys = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("ombor_kirim.xlsx") == 0).FirstOrDefault();
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
                                whName = row["Warehouse Name"].ToString();
                                partNo = row["Part Number"].ToString();
                                invoiceNo = row["Invoice No."].ToString();
                                waybillNo = row["Waybill No."].ToString();

                                PART_WRHS warehouse = db.PART_WRHS.Where(wh => wh.WHName.CompareTo(whName) == 0 && wh.IsDeleted == false).FirstOrDefault();
                                PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                P_INVOICES invoice = db.P_INVOICES.Where(pi => pi.InvoiceNo.CompareTo(invoiceNo) == 0 && pi.IsDeleted == false).FirstOrDefault();
                                F_WAYBILLS waybill = db.F_WAYBILLS.Where(wb => wb.WaybillNo.CompareTo(waybillNo) == 0 && wb.IsDeleted == false).FirstOrDefault();

                                P_WRHS_INCOMES income = db.P_WRHS_INCOMES.Where(inc => inc.InvoiceID == invoice.ID && inc.WHID == warehouse.ID && inc.IsDeleted == false).FirstOrDefault();

                                if (income != null)
                                {
                                    P_WRHS_INCOME_PARTS incomePart = db.P_WRHS_INCOME_PARTS.Where(incp => incp.IncomeID == income.ID && incp.PartID == part.ID).FirstOrDefault();
                                    if (incomePart != null)
                                    {
                                        ViewBag.ExistingRecordsCount = 1;
                                        if (CheckForInvoiceQty(incomePart, invoice) > 0)
                                            ViewBag.InvoiceCapacityExceedCount = 1;
                                    }
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
        private int CheckForInvoiceQty(P_WRHS_INCOME_PARTS incomePart, P_INVOICES invoice)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                P_INVOICE_PARTS invoicePart = db.P_INVOICE_PARTS.Where(invp => invp.InvoiceID == invoice.ID && invp.PartID == incomePart.PartID).FirstOrDefault();
                if (invoicePart != null)
                {
                    if (invoicePart.Quantity > incomePart.Amount)
                        return 0;
                    return 1;
                }
                else
                    return 0;
            }
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
            int a = 1, b = 0;
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
                            whName = row["Warehouse Name"].ToString();
                            partNo = row["Part Number"].ToString();
                            invoiceNo = row["Invoice No."].ToString();
                            waybillNo = row["Waybill No."].ToString();

                            PART_WRHS warehouse = db.PART_WRHS.Where(wh => wh.WHName.CompareTo(whName) == 0 && wh.IsDeleted == false).FirstOrDefault();
                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                            P_INVOICES invoice = db.P_INVOICES.Where(pi => pi.InvoiceNo.CompareTo(invoiceNo) == 0 && pi.IsDeleted == false).FirstOrDefault();
                            F_WAYBILLS waybill = db.F_WAYBILLS.Where(wb => wb.WaybillNo.CompareTo(waybillNo) == 0 && wb.IsDeleted == false).FirstOrDefault();

                            P_WRHS_INCOMES income = db.P_WRHS_INCOMES.Where(inc => inc.InvoiceID == invoice.ID && inc.WHID == warehouse.ID && inc.IssueDateTime.Day == DateTime.Now.Day && inc.IssueDateTime.Hour == DateTime.Now.Hour && inc.IssueDateTime.Minute == DateTime.Now.Minute && inc.IsDeleted == false).FirstOrDefault();

                            if (income == null)
                            {
                                P_WRHS_INCOMES new_income = new P_WRHS_INCOMES();

                                new_income.InvoiceID = invoice.ID;
                                new_income.WHID = warehouse.ID;
                                if (waybill != null)
                                    new_income.WaybillID = waybill.ID;
                                new_income.IssueDateTime = DateTime.Now;
                                new_income.Currency = row["Currency"].ToString();
                                new_income.IsDeleted = false;

                                P_WRHS_INCOMES lastIncome = db.P_WRHS_INCOMES.Where(inc => inc.InvoiceID == invoice.ID && inc.WHID == warehouse.ID && inc.IssueDateTime.Day == DateTime.Now.Day).OrderByDescending(icnp => icnp.IssueDateTime).FirstOrDefault();
                                if (lastIncome != null)
                                {
                                    b = Convert.ToInt32(lastIncome.DocNo.Substring(lastIncome.DocNo.LastIndexOf('_') + 1));
                                    a += b;
                                    docNo = whName + "_IN_" + DateTime.Now.ToString("ddMMyy") + "_" + a;
                                }
                                else
                                    docNo = whName + "_IN_" + DateTime.Now.ToString("ddMMyy") + "_" + a;
                                new_income.DocNo = docNo;
                                db.P_WRHS_INCOMES.Add(new_income);
                                db.SaveChanges();

                                P_WRHS_INCOME_PARTS incomePart = db.P_WRHS_INCOME_PARTS.Where(incp => incp.IncomeID == new_income.ID && incp.PartID == part.ID).FirstOrDefault();
                                if (incomePart == null)
                                {
                                    P_WRHS_INCOME_PARTS new_incomePart = new P_WRHS_INCOME_PARTS();
                                    new_incomePart.PartID = part.ID;
                                    new_incomePart.IncomeID = new_income.ID;
                                    new_incomePart.PiecePrice = Convert.ToDouble(row["Price"].ToString());
                                    new_incomePart.Unit = row["Unit"].ToString();
                                    new_incomePart.Amount = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_WRHS_INCOME_PARTS.Add(new_incomePart);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                P_WRHS_INCOME_PARTS incomePart = db.P_WRHS_INCOME_PARTS.Where(incp => incp.IncomeID == income.ID && incp.PartID == part.ID).FirstOrDefault();
                                if (incomePart == null)
                                {
                                    P_WRHS_INCOME_PARTS new_incomePart = new P_WRHS_INCOME_PARTS();
                                    new_incomePart.PartID = part.ID;
                                    new_incomePart.IncomeID = income.ID;
                                    new_incomePart.PiecePrice = Convert.ToDouble(row["Price"].ToString());
                                    new_incomePart.Unit = row["Unit"].ToString();
                                    new_incomePart.Amount = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_WRHS_INCOME_PARTS.Add(new_incomePart);
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