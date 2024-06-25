using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class POrderController : Controller
    {
        private DBTHSNEntities db2 = new DBTHSNEntities();
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
        private string supplierName, contractNo, orderNo, partNo = "";

        // GET: POrder
        public ActionResult Index(string type, int? supplierID)
        {
            IQueryable<P_ORDERS> ordersQuery = db2.P_ORDERS
                .Where(po => po.IsDeleted == false);

            if (!string.IsNullOrEmpty(type))
            {
                ViewBag.SourceList = new SelectList(sources, type);

                // Fetch SUPPLIER IDs matching the type
                List<int> supplierIds = db2.SUPPLIERS
                    .Where(s => s.Type == type && s.IsDeleted == false)
                    .Select(s => s.ID)
                    .ToList();

                // Filter orders by matching SUPPLIER IDs
                ordersQuery = ordersQuery
                    .Where(po => supplierIds.Contains((int)po.SupplierID));
            }
            else
            {
                ViewBag.SourceList = new SelectList(sources);
            }

            if (supplierID.HasValue)
            {
                ViewBag.SupplierList = new SelectList(db2.SUPPLIERS
                    .Where(s => s.ID == supplierID && s.IsDeleted == false)
                    .ToList(), "ID", "Name", supplierID);

                // Filter orders by supplierID
                ordersQuery = ordersQuery
                    .Where(po => po.SupplierID == supplierID);
            }
            else
            {
                ViewBag.SupplierList = new SelectList(db2.SUPPLIERS
                    .Where(s => s.IsDeleted == false)
                    .ToList(), "ID", "Name");
            }

            // Ensure Include is applied after filtering
            ordersQuery = ordersQuery.Include(po => po.P_CONTRACTS);
            ViewBag.Type = type;
            // Materialize the query into a list
            List<P_ORDERS> orders = ordersQuery.ToList();

            return View(orders);
        }




        public ActionResult Create()
        {
            /*using (DBTHSNEntities db = new DBTHSNEntities())
            {*/
                ViewBag.Supplier = new SelectList(db2.SUPPLIERS, "ID", "Name");
                ViewBag.PContract = new SelectList(db2.P_CONTRACTS, "ID", "ContractNo");
            //}

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderNo, IssuedDate, CompanyID, SupplierID, ContractID, Amount, Currency, Description")] P_ORDERS order)
        {
            /*using (DBTHSNEntities db = new DBTHSNEntities())
            {*/
                try
                {
                    if (ModelState.IsValid)
                    {
                        order.IsDeleted = false;
                        db2.P_ORDERS.Add(order);
                        db2.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, ex);
                }

                ViewBag.Supplier = new SelectList(db2.SUPPLIERS, "ID", "Name", order.SupplierID);
                ViewBag.PContract = new SelectList(db2.P_CONTRACTS, "ID", "ContractNo", order.ContractID);
                return View(order);
            //}
        }
        public ActionResult Details(int? id)
        {
            
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            P_ORDERS order;
            List<P_ORDER_PARTS> partList;
            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                order = db1.P_ORDERS.Find(id);
                if (order == null)
                    return HttpNotFound();

                db1.Entry(order).Reference(o => o.SUPPLIER).Load();
                db1.Entry(order).Reference(o => o.P_CONTRACTS).Load();

                partList = db1.P_ORDER_PARTS.Where(pc => pc.OrderID == id).ToList();
                foreach (var part in partList)
                {
                    db1.Entry(part).Reference(o => o.PART).Load();
                }
            }

            ViewBag.partList = partList;
            return View(order);
        }

        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            P_ORDERS order;
            List<P_ORDER_PARTS> partList;
            using (DBTHSNEntities db1 = new DBTHSNEntities())
            {
                order = db1.P_ORDERS.Find(Id);
                if (order == null)
                {
                    return HttpNotFound();
                }

                db1.Entry(order).Reference(o => o.SUPPLIER).Load();
                db1.Entry(order).Reference(o => o.P_CONTRACTS).Load();

                partList = db1.P_ORDER_PARTS.Where(op => op.OrderID ==  Id).ToList();
                foreach(var part in partList)
                    db1.Entry(part).Reference(p => p.PART).Load();
            }
            
            ViewBag.partList = partList;
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    P_ORDERS orderToDelete = db.P_ORDERS.Find(ID);
                    if (orderToDelete != null)
                    {
                        orderToDelete.IsDeleted = true;
                        try
                        {
                            db.Entry(orderToDelete).State = System.Data.Entity.EntityState.Modified;

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
                        ModelState.AddModelError("", "Bunday buyurtma topilmadi.");
                    }
                }
            }
            return View();
        }
        public ActionResult DeletePart(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                P_ORDER_PARTS orderPartToDelete = db.P_ORDER_PARTS.Find(id);
                if (ModelState.IsValid)
                {
                    if (orderPartToDelete != null)
                    {
                        try
                        {
                            db.P_ORDER_PARTS.Remove(orderPartToDelete);
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
                        ModelState.AddModelError("", "OrderPart not found.");
                    }
                }
                return View(orderPartToDelete);
            }
        }


        /* public ActionResult Edit(int? ID)
         {
             if (ID == null)
             {
                 return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
             }
             using (DBTHSNEntities db = new DBTHSNEntities())
             {
                 var order = db.P_ORDERS.Find(ID);
                 if (order == null)
                 {
                     return HttpNotFound();
                 }

                 db.Entry(order).Reference(o => o.SUPPLIER).Load();
                 db.Entry(order).Reference(o => o.P_CONTRACTS).Load();

                 ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name", order.SupplierID);
                 ViewBag.PContract = new SelectList(db.P_CONTRACTS, "ID", "ContractNo", order.ContractID);
                 ViewBag.partList = db.P_ORDER_PARTS.Where(pc => pc.OrderID == order.ID).ToList();

                 return View(order);
             }
         }*/
        public ActionResult Edit(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            /*using (DBTHSNEntities db = new DBTHSNEntities())
            {*/
                var order = db2.P_ORDERS.Find(ID);
                if (order == null)
                {
                    return HttpNotFound();
                }

                // Eager loading related entities
                db2.Entry(order).Reference(o => o.SUPPLIER).Load();
                db2.Entry(order).Reference(o => o.P_CONTRACTS).Load();
                db2.Entry(order).Collection(o => o.P_ORDER_PARTS).Query().Where(pc => pc.OrderID == order.ID).Load();

                // Populate ViewBag for dropdowns or other data needed in the view
                ViewBag.Supplier = new SelectList(db2.SUPPLIERS, "ID", "Name", order.SupplierID);
                ViewBag.PContract = new SelectList(db2.P_CONTRACTS, "ID", "ContractNo", order.ContractID);
                ViewBag.PartList = db2.P_ORDER_PARTS.Where(pc => pc.OrderID == order.ID).ToList();

                return View(order);
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(P_ORDERS order)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    P_ORDERS orderToUpdate = db.P_ORDERS.Find(order.ID);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.OrderNo = order.OrderNo;
                        orderToUpdate.SupplierID = order.SupplierID;
                        orderToUpdate.ContractID = order.ContractID;
                        orderToUpdate.IssuedDate = order.IssuedDate;
                        orderToUpdate.Currency = order.Currency;
                        orderToUpdate.Description = order.Description;

                        if (TryUpdateModel(orderToUpdate, "", new string[] { "OrderNo, IssuedDate, SupplierID, ContractID, Currency, Description" }))
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
                    return View(orderToUpdate);
                }
                // Re-populate the ViewBag.Supplier to ensure the dropdown list is available in case of an error
                ViewBag.Supplier = new SelectList(db.SUPPLIERS, "ID", "Name", order.SupplierID);
                ViewBag.PContract = new SelectList(db.P_CONTRACTS, "ID", "ContractNo", order.ContractID);
                ViewBag.partList = db.P_ORDER_PARTS.Where(pc => pc.OrderID == order.ID).ToList();
                return View(order);
            }
        }
        public ActionResult EditPart(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            /*using (DBTHSNEntities db = new DBTHSNEntities())
            {*/
                var orderPart = db2.P_ORDER_PARTS.Find(ID);
                if (orderPart == null)
                {
                    return HttpNotFound();
                }
                var allParts = db2.PARTS.Select(p => new SelectListItem
                {
                    Value = p.ID.ToString(),
                    Text = p.PNo
                }).ToList();

                ViewBag.PartList = allParts;

                return View(orderPart);
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPart(P_ORDER_PARTS orderPart)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    P_ORDER_PARTS orderPartToUpdate = db.P_ORDER_PARTS.Find(orderPart.ID);
                    if (orderPartToUpdate != null)
                    {
                        orderPartToUpdate.PartID = orderPart.PartID;
                        orderPartToUpdate.Price = orderPart.Price;
                        orderPartToUpdate.Amount = orderPart.Amount;
                        orderPartToUpdate.TotalPrice = orderPart.TotalPrice;
                        orderPartToUpdate.Unit = orderPart.Unit;
                        orderPartToUpdate.MOQ = orderPart.MOQ;
                        //orderPartToUpdate.Amount = orderPart.Quantity * orderPart.Price; SQL o'zi chiqarib beradi


                        if (TryUpdateModel(orderPartToUpdate, "", new string[] { "PartID, Price, Quantity, Unit, MOQ, TotalPrice" }))
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
                    return View(orderPartToUpdate);
                }
            }
            return View();
        }
        public async Task<ActionResult> Download()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                SAMPLE_FILES buyurtma = db.SAMPLE_FILES.Where(s => s.FileName.CompareTo("buyurtma.xlsx") == 0).FirstOrDefault();
                if (buyurtma != null)
                    return File(buyurtma.File, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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

                        using (DBTHSNEntities db = new DBTHSNEntities())
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                orderNo = row["OrderNo"].ToString();
                                contractNo = row["ContractNo"].ToString();
                                supplierName = row["Supplier Name"].ToString();
                                partNo = row["Part Number"].ToString();

                                SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                                PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                P_CONTRACTS contract = db.P_CONTRACTS.Where(p => p.ContractNo.CompareTo(contractNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                P_ORDERS order = db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.SupplierID == supplier.ID && po.ContractID == contract.ID && po.IsDeleted == false).FirstOrDefault();
                                if (order != null)
                                {
                                    P_ORDER_PARTS orderPart = db.P_ORDER_PARTS.Where(pop => pop.PartID == part.ID && pop.OrderID == order.ID).FirstOrDefault();
                                    if (orderPart != null)
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
                        using (DBTHSNEntities db = new DBTHSNEntities())
                        {
                            foreach (DataRow row in tableModel.Rows)
                            {
                                orderNo = row["OrderNo"].ToString();
                                contractNo = row["ContractNo"].ToString();
                                supplierName = row["Supplier Name"].ToString();
                                partNo = row["Part Number"].ToString();

                                SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                                PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                P_CONTRACTS contract = db.P_CONTRACTS.Where(p => p.ContractNo.CompareTo(contractNo) == 0 && p.IsDeleted == false).FirstOrDefault();

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
