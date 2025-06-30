using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using tahsinERP.Models;
using tahsinERP.ViewModels;
using tahsinERP.ViewModels.POrder;

namespace tahsinERP.Controllers
{
    public class POrderController : Controller
    {
        private string supplierName, contractNo, orderNo, partNo = "";
        private string[] sources;
        private int contractDocMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        private int unitID = 1;

        private List<string> missingContracts = new List<string>();
        private List<string> missingSuppliers = new List<string>();
        private List<string> missingParts = new List<string>();

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
        public JsonResult GetContractDetails(int contractID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contract = db.P_CONTRACTS
                                 .Where(c => c.ID == contractID && c.IsDeleted == false)
                                 .Select(c => new
                                 {
                                     c.ID,
                                     c.Currency
                                 })
                                 .FirstOrDefault();

                if (contract != null)
                {
                    return Json(new { success = true, data = contract }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Contract not found." }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult GetPriceAndMOQ(int partID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contractPart = db.P_CONTRACT_PARTS
                                     .Where(cp => cp.PartID == partID)
                                     .Select(cp => new
                                     {
                                         Price = cp.Price,
                                         MOQ = cp.MOQ,
                                         Amount = cp.Quantity
                                     })
                                     .FirstOrDefault();

                if (contractPart != null)
                {
                    return Json(new { success = true, data = contractPart }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Part not found in contract." }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        // Bu method tanlangan shartnomaga tegishli qismlar ro'yxatini qaytaradi
        public ActionResult GetPartList(int contractID)
        {
            using (var db = new DBTHSNEntities())
            {
                var partList = db.P_CONTRACT_PARTS
                    .Where(cp => cp.ContractID == contractID)
                    .Select(cp => new
                    {
                        cp.PartID,
                        cp.PART.PNo
                    })
                    .ToList();

                return Json(new { success = true, data = partList }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                //ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo");
                ViewBag.PContract = new SelectList(Enumerable.Empty<SelectListItem>());
                // Create a list of SelectListItem manually for parts
                ViewBag.partList = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(POrderCreateViewModel model)
        {
            try
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var isSameContract = db.P_CONTRACTS.Where(p => p.IsDeleted == false && p.SupplierID == model.SupplierID && p.ID == model.ContractID).FirstOrDefault();

                    if (isSameContract == null)
                    {
                        ModelState.AddModelError("", "Ta'minotchi va shartnomadagi ta'minotchi bir xil bo'lishi shart!");
                    }

                    foreach (var part in model.Parts)
                    {
                        if (part.MOQ > part.Amount)
                        {
                            ModelState.AddModelError("", db.PARTS.Where(p => p.ID == part.PartID).Select(p => p.PNo) + " uchun kiritilgan miqdor MOQ dan kichik");
                        }
                    }
                    if (checkForContractPartsAmount(model))
                    {
                        var message = "";
                        foreach (var word in contractExceedingParts)
                        {
                            message += word + " ,";
                        }
                        ModelState.AddModelError("", "Ushbu ehtiyot qismlarda " + message + " shartnomadan ortiqcha hajmni buyurtma qilmoqchisiz, bunday qilib bo'lmaydi!");

                    }
                    if (ModelState.IsValid)
                    {
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
                        LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{newOrder.OrderNo} - POrderni yaratdi");


                        List<P_CONTRACT_PARTS> contractParts = db.P_CONTRACT_PARTS.Where(pcp => pcp.ContractID == model.ContractID).ToList();
                        List<string> notInContractParts = new List<string>();

                        foreach (var part in model.Parts)
                        {
                            var contractPart = contractParts.Where(p => p.PartID == part.PartID).FirstOrDefault();
                            if (contractParts.Where(cp => cp.PartID == part.PartID).Any())
                            {
                                var newPart = new P_ORDER_PARTS
                                {
                                    OrderID = newOrder.ID,
                                    PartID = part.PartID,
                                    UnitID = part.UnitID,
                                    Amount = part.Amount
                                };

                                newPart.MOQ = contractPart.MOQ;
                                newPart.Price = contractPart.Price;

                                db.P_ORDER_PARTS.Add(newPart);
                                var logPart = db.PARTS.Find(newPart.PartID);
                                LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{logPart.PNo} - POrderPartni yaratdi");
                            }
                            else
                            {
                                PART paart = db.PARTS.Where(p => p.ID == part.PartID).FirstOrDefault();
                                notInContractParts.Add(paart.PNo);
                            }
                        }
                        if (notInContractParts.Count > 0)
                        {
                            var message = "";
                            foreach (var word in notInContractParts)
                            {
                                message += word + " ,";
                            }
                            ModelState.AddModelError("", "Ushbu ehtiyot qism(lar): " + message + " shartnomadan topilmadi, shartnomada yo'q narsaga buyurtma qilib bo'lmaydi! Qaytadan urinib ko'ring!");
                            db.Entry(newOrder).State = EntityState.Deleted;
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
                                        P_ORDER_DOCS orderDoc = new P_ORDER_DOCS();
                                        byte[] avatar = new byte[Request.Files["docUpload"].InputStream.Length];
                                        Request.Files["docUpload"].InputStream.Read(avatar, 0, avatar.Length);
                                        orderDoc.OrderID = newOrder.ID;
                                        orderDoc.Doc = avatar;

                                        db.P_ORDER_DOCS.Add(orderDoc);
                                        db.SaveChanges();

                                        LogHelper.LogToDatabase(User.Identity.Name, "PContractController", $"{orderDoc.P_ORDERS.OrderNo} - uchun POrderDocni yaratdi");
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

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Supplier = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                        ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo");
                        ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                        //ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
                        ViewBag.partList = new SelectList(Enumerable.Empty<SelectList>());

                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }
        

        private List<string> contractExceedingParts = new List<string>();
        private bool checkForContractPartsAmount(POrderCreateViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                List<P_CONTRACT_PARTS> contractParts = db.P_CONTRACT_PARTS.Where(pcp => pcp.ContractID == model.ContractID).ToList();

                foreach (var orderPart in model.Parts)
                {
                    for (int i = 0; i < contractParts.Count; i++)
                    {
                        if (contractParts[i].PartID == orderPart.PartID)
                            if (orderPart.Amount > contractParts[i].Quantity)
                                contractExceedingParts.Add(contractParts[i].PART.PNo);
                    }
                }
                if (contractExceedingParts.Count > 0)
                    return true;
                else
                    return false;
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

                    LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{newOrder.OrderNo} - POrderni yaratdi");

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
                                PNo = part.Standart + "" + part.Thickness + "x" + part.Width,
                                IsInHouse = false,
                                IsDeleted = false,
                                PWidth = part.Width,
                                UnitID = part.UnitID,
                                Type = "Steel"
                            };
                            db.PARTS.Add(newPartInsert);
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{newPartInsert.PNo} - Partni yaratdi");

                            var newOrderPart = new P_ORDER_PARTS
                            {
                                OrderID = newOrderID,
                                PartID = newPartInsert.ID,
                                UnitID = newPartInsert.UnitID,
                                //Amount = part.Amount,
                            };
                            db.P_ORDER_PARTS.Add(newOrderPart);
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{newOrderPart.PART.PNo} - POrderPartni yaratdi");
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

                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                    return View(model);
                }

                return RedirectToAction("Index");
            }
        }
        public ActionResult DownloadDoc(int? orderID)
        {
            if (orderID == null)
            {
                return Json(new { success = false, message = "Buyurtma ID ko'rsatilmagan." }, JsonRequestBehavior.AllowGet);
            }
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var orderDoc = db.P_ORDER_DOCS
                                    .Include(x => x.P_ORDERS)
                                    .FirstOrDefault(pi => pi.OrderID == orderID);
                if (orderDoc != null)
                {
                    string orderNo = orderDoc.P_ORDERS.OrderNo;
                    return File(orderDoc.Doc, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", orderNo + "_OrderDoc.pdf");
                }
                else
                {
                    return Json(new { success = false, message = "buyurtma uchun fayl yuklanmagan." }, JsonRequestBehavior.AllowGet);
                }
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
                    .Where(op => op.OrderID == Id).ToList();
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
                            db.Entry(orderToDelete).State = EntityState.Modified;

                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{orderToDelete.OrderNo} - POrderni o'chirdi");

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

                            LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{orderPartToDelete.PART.PNo} - POrderPartni o'chirdi");

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
                var order = db.P_ORDERS.Include(x => x.P_ORDER_PARTS).Where(x => x.ID == ID && x.IsDeleted == false).FirstOrDefault();
                if (order == null)
                {
                    return HttpNotFound();
                }

                var model = new POrderViewModel
                {
                    ID = order.ID,
                    OrderNo = order.OrderNo,
                    SupplierID = (int)order.SupplierID,
                    ContractID = (int)order.ContractID,
                    Currency = order.Currency,
                    Description = order.Description,
                    IssuedDate = order.IssuedDate,
                    Parts = order.P_ORDER_PARTS.Select(p => new ViewModels.POrderPart
                    {
                        ID = p.ID,
                        OrderID = p.OrderID,
                        Part = p.PART,
                        PartID = p.PartID,
                        UnitID = (int)p.UnitID,
                        Amount = (double)p.Amount,
                        MOQ = (double)p.MOQ,
                        Price = (float)p.Price,
                    }).ToList()
                };

                // Qismlar uchun SelectList yaratish
                ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");

                ViewBag.Suppliers = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", order.SupplierID);
                ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", order.ContractID);
                ViewBag.Units = new SelectList(db.UNITS.ToList(), "ID", "ShortName");

                ViewBag.partList = new SelectList(db.PARTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");

                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(POrderViewModel order)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Re-populate the ViewBag.Supplier to ensure the dropdown list is available in case of an error
                ViewBag.Suppliers = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", order.SupplierID);
                ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", order.ContractID);
                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                /*if (ModelState.IsValid)
                {*/
                // Re-populate the ViewBag.Supplier to ensure the dropdown list is available in case of an error
                ViewBag.Suppliers = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", order.SupplierID);
                ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", order.ContractID);

                ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");

                var isSameContract = db.P_CONTRACTS
                        .Include(x => x.SUPPLIER)
                        .Where(p => p.IsDeleted == false && p.ID == order.ContractID)
                        .FirstOrDefault();

                if (order.SupplierID != isSameContract.SupplierID)
                {
                    ModelState.AddModelError("", "Ta'minotchi and shartnomadagi ta'minotchi bir xil emas!");
                    // Re-populate the ViewBag.Supplier to ensure the dropdown list is available in case of an error
                    ViewBag.Suppliers = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", order.SupplierID);
                    ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", order.ContractID);

                    ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
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

                        LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{orderToUpdate.OrderNo} - POrderni tahrirladi");
                    }
                    catch (RetryLimitExceededException)
                    {
                        ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        // Re-populate the ViewBag.Supplier to ensure the dropdown list is available in case of an error
                        ViewBag.Suppliers = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", order.SupplierID);
                        ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", order.ContractID);
                    }

                    // Update ProductList
                    foreach (var product in order.Parts)
                    {
                        var existingPart = db.P_ORDER_PARTS.Find(product.ID);
                        if (existingPart != null)
                        {
                            existingPart.Price = product.Price;
                            existingPart.UnitID = product.UnitID;
                            existingPart.Amount = product.Amount;
                            existingPart.MOQ = product.MOQ;

                            db.Entry(existingPart).State = EntityState.Modified;

                            LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{existingPart.PART.PNo} - POrderPartni tahrirladi");
                        }
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (RetryLimitExceededException)
                    {
                        ModelState.AddModelError("", "Extiyot qism o'zgarishlarini saqlab bo'lmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratsiyasiga muroaat qiling.");
                        // Re-populate the ViewBag.Supplier to ensure the dropdown list is available in case of an error
                        ViewBag.Suppliers = new SelectList(db.SUPPLIERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", order.SupplierID);
                        ViewBag.PContract = new SelectList(db.P_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo", order.ContractID);

                        ViewBag.units = new SelectList(db.UNITS.ToList(), "ID", "UnitName");
                    }

                    return RedirectToAction("Index");
                }

                return View();
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
                        var summ = db.P_ORDER_PARTS.Where(x => x.OrderID == order.ID && x.OrderID != orderPartToUpdate.ID).Sum(p => p.Amount);
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

                            LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{orderPartToUpdate.PART.PNo} - POrderPartni tahrirladi");

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

        private async Task<bool> CheckForExistenceOfContracts(DataTable dataTable)
        {
            if (dataTable == null)
                return false;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    string contractNo = row["ContractNo"].ToString();

                    bool contractExists = await db.P_CONTRACTS.AnyAsync(p => p.ContractNo.CompareTo(contractNo) == 0);
                    if (!contractExists)
                    {
                        if (!missingContracts.Contains(contractNo))
                        {
                            missingContracts.Add(contractNo);
                        }
                        return false;  // Return false immediately if any contract is missing
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

                        if (await CheckForExistenceOfContracts(dataTable))
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
                                            contractNo = row["ContractNo"].ToString();
                                            supplierName = row["Supplier Name"].ToString();
                                            partNo = row["Part Number"].ToString();

                                            SUPPLIER supplier = db.SUPPLIERS.Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false).FirstOrDefault();
                                            PART part = db.PARTS.Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                            P_CONTRACTS contract = db.P_CONTRACTS.Where(p => p.ContractNo.CompareTo(contractNo) == 0 && p.IsDeleted == false).FirstOrDefault();
                                            P_ORDERS order = db.P_ORDERS.Where(po => po.OrderNo.CompareTo(orderNo) == 0 && po.SupplierID == supplier.ID && po.ContractID == contract.ID && po.IsDeleted == false).FirstOrDefault();
                                            if (order != null && supplier != null && part != null)
                                            {
                                                P_ORDER_PARTS orderPart = db.P_ORDER_PARTS
                                                    .Where(pop => pop.PartID == part.ID && pop.OrderID == order.ID)
                                                    .FirstOrDefault();
                                                if (orderPart != null)
                                                    ViewBag.ExistingRecordsCount = 1;
                                            }
                                            else
                                            {
                                                ViewBag.Message = "Order/Supplier/Part topilmadi";
                                                return View("UploadWithExcel");
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
                                    ViewBag.Message = "Ushbu buyurtmalar faylida kiritilgan ehtiyot qismlar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Ehtiyot qismlar bazasiga kiritib keyin qayta urining.";
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
                                ViewBag.Message = "Ushbu buyurtmalar faylda kiritilgan ta'minotchilar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Ta'minotchilar bazasiga kiritib keyin qayta urining.";
                                return View("UploadWithExcel");
                            }
                        }
                        else
                        {
                            var message = "";
                            foreach (var word in missingContracts)
                            {
                                message += word + ", ";
                            }
                            ViewBag.Message = "Ushbu buyurtmalar faylda kiritilgan shartnomalar: " + message + " tizim bazasida mavjud emas. Qaytadan tekshiring, avval Shartnomalar bazasiga kiritib keyin qayta urining.";
                            return View("UploadWithExcel");
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = $"Faylni yuklashda quyidagicha muammo tug'ildi: {ex.Message}";
                        if (ex.InnerException != null)
                        {
                            ViewBag.Message += $" - Ichki xato: {ex.InnerException.Message}";
                        }
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
                            try
                            {
                                string orderNo = row["OrderNo"].ToString();
                                string contractNo = row["ContractNo"].ToString();
                                string supplierName = row["Supplier Name"].ToString();
                                string partNo = row["Part Number"].ToString();

                                SUPPLIER supplier = db.SUPPLIERS
                                    .Where(s => s.Name.CompareTo(supplierName) == 0 && s.IsDeleted == false)
                                    .FirstOrDefault();

                                PART part = db.PARTS
                                    .Where(p => p.PNo.CompareTo(partNo) == 0 && p.IsDeleted == false)
                                    .FirstOrDefault();

                                P_CONTRACTS contract = db.P_CONTRACTS
                                    .Where(p => p.ContractNo.CompareTo(contractNo) == 0 && p.IsDeleted == false)
                                    .FirstOrDefault();

                                P_ORDERS order = db.P_ORDERS
                                    .Where(po => po.OrderNo == orderNo && po.IsDeleted == false)
                                    .FirstOrDefault();
                                if (order == null)
                                {
                                    P_ORDERS new_order = new P_ORDERS
                                    {
                                        OrderNo = orderNo,
                                        ContractID = contract.ID,
                                        SupplierID = supplier.ID,
                                        IssuedDate = DateTime.Parse(row["IssuedDate"].ToString()),
                                        Currency = row["Currency"].ToString(),
                                        CompanyID = Convert.ToInt32(ConfigurationManager.AppSettings["companyID"]),
                                        IsDeleted = false
                                    };

                                    db.P_ORDERS.Add(new_order);
                                    await db.SaveChangesAsync();

                                    LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{new_order.OrderNo} - POrderni Excell orqali yaratdi");

                                    string unitFromFile = row["Unit"].ToString();
                                    UNIT unit = db.UNITS.Where(u => u.ShortName.CompareTo(unitFromFile) == 0).FirstOrDefault();
                                    if (unit != null)
                                        unitID = unit.ID;

                                    P_ORDER_PARTS orderPart = db.P_ORDER_PARTS
                                        .Where(pcp => pcp.OrderID == new_order.ID && pcp.PartID == part.ID)
                                        .FirstOrDefault();
                                    if (orderPart == null)
                                    {
                                        P_ORDER_PARTS new_orderPart = new P_ORDER_PARTS
                                        {
                                            PartID = part.ID,
                                            OrderID = new_order.ID,
                                            Price = Convert.ToDouble(row["Price"].ToString()),
                                            MOQ = Convert.ToDouble(row["MOQ"].ToString()),
                                            Amount = Convert.ToDouble(row["Amount"].ToString()),
                                            UnitID = unitID
                                        };

                                        db.P_ORDER_PARTS.Add(new_orderPart);
                                        await db.SaveChangesAsync();

                                        var logPart = db.PARTS.Find(new_orderPart.PartID);
                                        LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{logPart.PNo} - POrderPartni Excell orqali yaratdi");
                                    }
                                }
                                else
                                {
                                    P_ORDER_PARTS orderPart = db.P_ORDER_PARTS
                                        .Where(pcp => pcp.OrderID == order.ID && pcp.PartID == part.ID)
                                        .FirstOrDefault();
                                    if (orderPart == null)
                                    {
                                        string unitFromFile = row["Unit"].ToString();
                                        UNIT unit = db.UNITS.Where(u => u.ShortName.CompareTo(unitFromFile) == 0).FirstOrDefault();
                                        if (unit != null)
                                            unitID = unit.ID;

                                        P_ORDER_PARTS new_orderPart = new P_ORDER_PARTS
                                        {
                                            PartID = part.ID,
                                            OrderID = order.ID,
                                            Price = Convert.ToDouble(row["Price"].ToString()),
                                            MOQ = Convert.ToDouble(row["MOQ"].ToString()),
                                            Amount = Convert.ToDouble(row["Amount"].ToString()),
                                            UnitID = unitID
                                        };

                                        db.P_ORDER_PARTS.Add(new_orderPart);
                                        await db.SaveChangesAsync();
                                        var logPart = db.PARTS.Find(new_orderPart.PartID);
                                        LogHelper.LogToDatabase(User.Identity.Name, "POrderController", $"{logPart.PNo} - POrderPartni Excell orqali yaratdi");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("", $"Xatolik: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Xatolik: {ex.Message}");
                }
            }

            return RedirectToAction("Index");
        }
    }
}