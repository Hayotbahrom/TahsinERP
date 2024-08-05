using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
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
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class POrderController : Controller
    {
        
        private string supplierName, contractNo, orderNo, partNo = "";
        private string[] sources;
        public POrderController()
        {
            sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
            sources = sources.Where(x => !x.Equals("InHouse", StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        // GET: POrder
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var suppliers = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
                if (!string.IsNullOrEmpty(type))
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false && s.SupplierID == supplierID && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.Type.CompareTo(type) == 0), "ID", "Name", supplierID);
                    }
                    else
                    {
                        ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.Type.CompareTo(type) == 0), "ID", "Name");
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false && s.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name", supplierID);
                    }
                    else
                    {
                        ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false).ToList();
                        ViewBag.SourceList = new SelectList(sources);
                        ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name");
                    }
                }
                return View();
            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(POrderViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");

                var isSameContract = db.P_CONTRACTS
                    .Include(x => x.SUPPLIER)
                    .Where(p => p.IsDeleted == false && p.ID == model.ContractID).FirstOrDefault();

                if (model.SupplierID != isSameContract.SupplierID)
                {
                    ModelState.AddModelError("", "Ta'minotchi va shartnomadagi ta'minotchi bir xil bo'lishi shart!");
                }

                foreach (var part in model.Parts)
                {
                    if (part.MOQ > part.Amount)
                    {
                        ModelState.AddModelError("", "Kiritilgan miqdor MOQ dan kichik");
                    }
                }
                var summ = model.Parts.Sum(x => x.Amount);
                var contractAmount = db.P_CONTRACTS.Where(x => x.IsDeleted == false && x.ID == model.ContractID).FirstOrDefault().Amount;
                if (summ>contractAmount)
                {
                    ModelState.AddModelError("", "Shartnomadan ortiqcha hajmni buyurtma qilib bo'lmaydi");
                }
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var newOrder = new P_ORDERS()
                {
                    OrderNo = model.OrderNo,
                    IssuedDate = model.IssuedDate,
                    CompanyID = 1,
                    SupplierID = model.SupplierID,
                    ContractID = model.ContractID,
                    Currency = model.Currency,
                    Description = model.Description,
                    IsDeleted = false
                };

                db.P_ORDERS.Add(newOrder);
                db.SaveChanges();

                int newOrderID = newOrder.ID;

                foreach (var part in model.Parts)
                {
                    var newPart = new P_ORDER_PARTS
                    {
                        OrderID = newOrderID,
                        PartID = part.PartID,
                        UnitID = part.UnitID,
                        Amount = part.Amount,
                        Price = part.Price,
                        MOQ = part.MOQ
                    };

                    db.P_ORDER_PARTS.Add(newPart);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        public ActionResult GetContractsBySupplier(int supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contracts = db.P_CONTRACTS
                    .Where(x => x.IsDeleted == false && x.SupplierID == supplierID)
                    .Select(x => new { x.ID, x.ContractNo })
                    .ToList();

                return Json(contracts.Select(c => new SelectListItem { Value = c.ID.ToString(), Text = c.ContractNo }), JsonRequestBehavior.AllowGet);
            }
        }

        //steel coil uchun Create Actoin method
        public ActionResult CreateSteel(int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (supplierID.HasValue)
                {
                    ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false && x.Type.CompareTo("Steel") == 0).ToList(), "ID", "Name");
                    ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false && x.SupplierID == supplierID.Value).ToList(), "ID", "ContractNo");
                }
                else
                {
                    ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false && x.Type.CompareTo("Steel") == 0).ToList(), "ID", "Name");
                    ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo");
                }
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                var steelCoils = db.STEEL_COILS.ToList();

                ViewBag.steelMarka = new SelectList(steelCoils.ToList(), "ID", "Marka");
                ViewBag.steelStandart = new SelectList(steelCoils.ToList(), "ID", "Standart");
                ViewBag.steelCoating = new SelectList(steelCoils.ToList(), "ID", "Coating");
                ViewBag.steelThickness = new SelectList(steelCoils.ToList(), "ID", "Thickness");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSteel(POrderSteelViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    var steelCoil = db.STEEL_COILS.ToList();

                    var newOrder = new P_ORDERS()
                    {
                        OrderNo = model.OrderNo,
                        IssuedDate = model.IssuedDate,
                        CompanyID = 1,
                        SupplierID = model.SupplierID,
                        ContractID = model.ContractID,
                        Currency = model.Currency,
                        Description = model.Description,
                        IsDeleted = false
                    };
                    db.P_ORDERS.Add(newOrder);
                    db.SaveChanges();

                    // Yangi yozuvning IncomeID sini olish
                    int newOrderID = newOrder.ID;

                    // Parts ni saqlash
                    foreach (var part in model.Parts)
                    {
                        var existPart = db.PARTS.Where(p => p.IsDeleted == false &&
                                                            p.Marka == part.Marka &&
                                                            p.Coating == part.Coating &&
                                                            p.Standart == part.Standart 
                                                            && p.Gauge == part.Thickness).FirstOrDefault();
                        if (existPart is null)
                        {
                            var newPartInsert = new PART
                            {
                                Marka = part.Marka,
                                Coating = part.Coating,
                                Standart = part.Standart,
                                PNo = part.Standart + "" + part.Thickness+"x"+part.Width,
                                IsInHouse = false,
                                IsDeleted = false
                            };
                            db.PARTS.Add(newPartInsert);
                            db.SaveChanges();

                            var newOrderPart = new P_ORDER_PARTS
                            {
                                OrderID = newOrderID,
                                PartID = newPartInsert.ID,
                                UnitID = newPartInsert.UnitID
                            };
                            db.P_ORDER_PARTS.Add(newOrderPart);
                            db.SaveChanges();
                        }
                        else
                        {
                            var existNewOrderPart = new P_ORDER_PARTS
                            {
                                OrderID = newOrderID
                            };
                            db.P_ORDER_PARTS.Add(existNewOrderPart);
                        }
                    }
                }
                catch
                {
                    ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", model.ContractID);
                    ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false && x.Type.CompareTo("Steel") == 0).ToList(), "ID", "Name", model.SupplierID);
                    ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                    ViewBag.steelMarka = new SelectList(db.STEEL_COILS.ToList(), "ID", "Marka");
                    ViewBag.steelStandart = new SelectList(db.STEEL_COILS.ToList(), "ID", "Standart");
                    ViewBag.steelCoating = new SelectList(db.STEEL_COILS.ToList(), "ID", "Coating");
                    ViewBag.steelThickness = new SelectList(db.STEEL_COILS.ToList(), "ID", "Thickness");

                    return View(model);
                }

                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "POrderController", "CreateSteel[Post]");
                return RedirectToAction("Index");
            }
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
                    db1.Entry(part).Reference(o => o.UNIT).Load();
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

                partList = db1.P_ORDER_PARTS
                    .Include(pc => pc.UNIT)
                    .Include(pc => pc.PART)
                    .Where(op => op.OrderID ==  Id).ToList();
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
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "POrderController", "Delete[Post]");
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
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "POrderController", "DeletePart[Post]");
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
        public ActionResult Edit(int? ID)
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

                // Eager loading related entities
                db.Entry(order).Reference(o => o.SUPPLIER).Load();
                db.Entry(order).Reference(o => o.P_CONTRACTS).Load();
                db.Entry(order).Collection(o => o.P_ORDER_PARTS).Query().Where(pc => pc.OrderID == order.ID).Load();

                // Populate ViewBag for dropdowns or other data needed in the view
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.ToList(), "ID", "Name", order.SupplierID);
                ViewBag.PContract = new SelectList(db.P_CONTRACTS.ToList(), "ID", "ContractNo", order.ContractID);
                ViewBag.PartList = db.P_ORDER_PARTS
                                .Include(pc => pc.PART)
                                .Include(pc => pc.UNIT)
                                .Where(pc => pc.OrderID == order.ID).ToList();

                return View(order);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(P_ORDERS order)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    // Re-populate the ViewBag.Supplier to ensure the dropdown list is available in case of an error
                    ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", order.SupplierID);
                    ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", order.ContractID);
                    ViewBag.partList = db.P_ORDER_PARTS
                                                .Include(x => x.PART)                    
                                                .Where(pc => pc.OrderID == order.ID).ToList();
                    ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                    var isSameContract = db.P_CONTRACTS
                    .Include(x => x.SUPPLIER)
                   .Where(p => p.IsDeleted == false && p.ID == order.ContractID).FirstOrDefault();

                    if (order.SupplierID != isSameContract.SupplierID)
                    {
                        ModelState.AddModelError("", "Ta'minotchi and shartnomadagi ta'minotchi bir xil emas!");
                    }

                    if (!ModelState.IsValid)
                    {
                        return View(order);
                    }
                    P_ORDERS orderToUpdate = db.P_ORDERS.Find(order.ID);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.OrderNo = order.OrderNo;
                        orderToUpdate.SupplierID = order.SupplierID;
                        orderToUpdate.ContractID = order.ContractID;
                        orderToUpdate.IssuedDate = order.IssuedDate;
                        orderToUpdate.Currency = order.Currency;
                        orderToUpdate.Description = order.Description;

                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "POrderController", "Edit[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    return View(orderToUpdate);
                }

                return View(order);
            }
        }
        public ActionResult EditPart(int? ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var orderPart = db.P_ORDER_PARTS.Find(ID);
                if (orderPart == null)
                {
                    return HttpNotFound();
                }
                var allParts = db.PARTS.Where(x => x.IsDeleted == false).ToList();

                db.Entry(orderPart).Reference(o => o.P_ORDERS).Load();
                db.Entry(orderPart).Reference(o => o.UNIT).Load();
                ViewBag.partList = new SelectList(allParts, "ID", "PNo");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                return View(orderPart);
            }
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
                        orderPartToUpdate.UnitID = orderPart.UnitID;
                        orderPartToUpdate.MOQ = orderPart.MOQ;
                        //orderPartToUpdate.Amount = orderPart.Quantity * orderPart.Price; SQL o'zi chiqarib beradi
                        if (orderPart.MOQ > orderPart.Amount)
                        {
                            ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
                            ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                            
                            ModelState.AddModelError("", "Kiritilgan miqdor MOQ dan kichik");
                            return View(orderPart);
                        }

                        //shartnomadagi miqdor va editpart dagi miqdorni taqqoslash
                        var order = db.P_ORDERS.Where(x => x.ID == orderPartToUpdate.OrderID).FirstOrDefault();
                        var summ = db.P_ORDER_PARTS.Where(x => x.OrderID == order.ID && x.OrderID!=orderPartToUpdate.ID).Sum(p => p.Amount);
                        summ += orderPart.Amount;
                        var contractAmount = db.P_CONTRACTS.Where(x => x.IsDeleted == false && x.ID == order.ContractID).FirstOrDefault().Amount;
                        if (summ > contractAmount)
                        {
                            ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
                            ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                            ModelState.AddModelError("", "Shartnomadan ortiqcha hajmni buyurtma qilib bo'lmaydi");
                            return View(orderPart);
                        }

                        try
                        {
                            db.SaveChanges();
                            var userEmail = User.Identity.Name;
                            LogHelper.LogToDatabase(userEmail, "POrderController", "EditPart[Post]");
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
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
                    var tableModel = JsonConvert.DeserializeObject<System.Data.DataTable>(dataTableModel);

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
                                        //new_orderPart.Unit = row["Unit"].ToString();
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
                                        //new_orderPart.Unit = row["Unit"].ToString();
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

            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "POrderController", "Save[Post]");
            return RedirectToAction("Index");
        }
    }
}
