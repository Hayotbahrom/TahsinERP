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
    public class POrderController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private readonly string[] sources = new string[4] { "", "KD", "Steel", "Maxalliy" };
        private string supplierName, contractNo, orderNo, partNo = "";

        // GET: POrder
        public ActionResult Index(string type, int? supplierID)
        {
            if (!string.IsNullOrEmpty(type))
            {
                if (supplierID.HasValue)
                {
                    List<P_ORDERS> list = db.P_ORDERS.Where(po => po.IsDeleted == false && po.SUPPLIER.Type.CompareTo(type) == 0 && po.SupplierID == supplierID).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name", supplierID);

                    return View(list);
                }
                else
                {
                    List<P_ORDERS> list = db.P_ORDERS.Where(po => po.IsDeleted == false && po.SUPPLIER.Type.CompareTo(type) == 0).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name");

                    return View(list);
                }
            }
            else
            {
                if (supplierID.HasValue)
                {
                    List<P_ORDERS> list = db.P_ORDERS.Where(po => po.IsDeleted == false && po.SupplierID == supplierID).ToList();
                    ViewBag.SourceList = new SelectList(sources);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                    return View(list);
                }
                else
                {
                    List<P_ORDERS> list = db.P_ORDERS.Where(po => po.IsDeleted == false).ToList();
                    ViewBag.SourceList = new SelectList(sources);
                    ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name");
                    return View(list);
                }
            }
        }
        public ActionResult Create()
        {
            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name");
            ViewBag.PContract = new SelectList(db.P_CONTRACTS, "ID", "ContractNo");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderNo, IssuedDate, CompanyID, SupplierID, ContractID, Amount, Currency, Description")] P_ORDERS order)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    order.IsDeleted = false;
                    db.P_ORDERS.Add(order);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }

            ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name", order.SupplierID);
            ViewBag.PContract = new SelectList(db.P_CONTRACTS, "ID", "ContractNo", order.ContractID);
            return View(order);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var order = db.P_ORDERS.Find(id);
            if (order == null)
                return HttpNotFound();


            return View();
        }
        public async Task<ActionResult> Download()
        {
            SAMPLE_FILES buyurtma = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("buyurtma.xlsx") == 0).FirstOrDefault();
            if (buyurtma != null)
                return File(buyurtma.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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

                        foreach (DataRow row in dataTable.Rows)
                        {
                            orderNo = row["OrderNo"].ToString();
                            contractNo = row["ContractNo"].ToString();
                            supplierName = row["Supplier Name"].ToString();
                            partNo = row["Part Number"].ToString();

                            SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0).FirstOrDefault();
                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0).FirstOrDefault();
                            P_CONTRACTS contract = db.P_CONTRACTS.Where(p => p.ContractNo.CompareTo(contractNo) == 0).FirstOrDefault();
                            P_ORDERS order = db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.SupplierID == supplier.ID && po.ContractID == contract.ID && po.IsDeleted == false).FirstOrDefault();
                            if (order != null)
                            {
                                P_ORDER_PARTS orderPart = db.P_ORDER_PARTS.Where(pop => pop.PartID == part.ID && pop.OrderID == order.ID).FirstOrDefault();
                                if (orderPart != null)
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

            // Perform CPU-bound work here
            // For example, heavy computations or other synchronous tasks
            ViewBag.DataTable = null;
            ViewBag.DataTableModel = null;
            ViewBag.IsFileUploaded = false;
            ViewBag.Message = "Jadval ma'lumotlari o'chirib yuborildi.";


            return View("UploadWithExcel");
        }
        [HttpPost]
        public async Task<ActionResult> Save(string dataTableModel)
        {

            // Perform CPU-bound work here
            // For example, heavy computations or other synchronous tasks

            if (!string.IsNullOrEmpty(dataTableModel))
            {
                await Task.Run(() =>
                {
                    var tableModel = JsonConvert.DeserializeObject<DataTable>(dataTableModel);

                    try
                    {
                        foreach (DataRow row in tableModel.Rows)
                        {
                            orderNo = row["OrderNo"].ToString();
                            contractNo = row["ContractNo"].ToString();
                            supplierName = row["Supplier Name"].ToString();
                            partNo = row["Part Number"].ToString();

                            SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0).FirstOrDefault();
                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0).FirstOrDefault();
                            P_CONTRACTS contract = db.P_CONTRACTS.Where(p => p.ContractNo.CompareTo(contractNo) == 0).FirstOrDefault();

                            P_ORDERS order = db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.IsDeleted == false).FirstOrDefault();
                            if (order == null)
                            {
                                P_ORDERS new_order = new P_ORDERS();
                                new_order.OrderNo = orderNo;
                                new_order.ContractID = contract.ID;
                                new_order.SupplierID = supplier.ID;
                                new_order.IssuedDate = DateTime.Parse(row["IssuedDate"].ToString());
                                new_order.Currency = row["Currency"].ToString();
                                new_order.CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["companyID"]);
                                new_order.IsDeleted = false;

                                db.P_ORDERS.Add(new_order);
                                db.SaveChanges();

                                P_ORDER_PARTS orderPart = db.P_ORDER_PARTS.Where(pcp => pcp.OrderID == new_order.ID && pcp.PartID == part.ID).FirstOrDefault();
                                if (orderPart == null)
                                {
                                    P_ORDER_PARTS new_orderPart = new P_ORDER_PARTS();
                                    new_orderPart.PartID = part.ID;
                                    new_orderPart.OrderID = new_order.ID;
                                    new_orderPart.Price = Convert.ToDouble(row["Price"].ToString());
                                    new_orderPart.Unit = row["Unit"].ToString();
                                    new_orderPart.MOQ = Convert.ToDouble(row["MOQ"].ToString());
                                    new_orderPart.Amount = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_ORDER_PARTS.Add(new_orderPart);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                P_ORDER_PARTS orderPart = db.P_ORDER_PARTS.Where(pcp => pcp.OrderID == order.ID && pcp.PartID == part.ID).FirstOrDefault();
                                if (orderPart == null)
                                {
                                    P_ORDER_PARTS new_orderPart = new P_ORDER_PARTS();
                                    new_orderPart.PartID = part.ID;
                                    new_orderPart.OrderID = order.ID;
                                    new_orderPart.Price = Convert.ToDouble(row["Price"].ToString());
                                    new_orderPart.Unit = row["Unit"].ToString();
                                    new_orderPart.MOQ = Convert.ToDouble(row["MOQ"].ToString());
                                    new_orderPart.Amount = Convert.ToDouble(row["Amount"].ToString());

                                    db.P_ORDER_PARTS.Add(new_orderPart);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                });
            }
            return RedirectToAction("Index");
        }
    }
}
