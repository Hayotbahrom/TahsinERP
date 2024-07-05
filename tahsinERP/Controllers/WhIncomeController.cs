using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

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
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0 && pi.P_INVOICES.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<PART_WRHS_INCOMES> list = db.PART_WRHS_INCOMES.Include(pr => pr.P_INVOICES).Include(pr => pr.F_WAYBILLS).Where(pi => pi.IsDeleted == false).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Wrhs = new SelectList(db.PART_WRHS.Where(w => w.IsDeleted == false).ToList(), "ID", "WHName");
                ViewBag.Invoices = new SelectList(db.P_INVOICES.Where(i => i.IsDeleted == false).ToList(), "ID", "InvoiceNo");
                ViewBag.Waybills = new SelectList(db.F_WAYBILLS.Where(w => w.IsDeleted == false).ToList(), "ID", "WaybillNo");

                ViewBag.InComes = new SelectList(db.PART_WRHS_INCOMES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
                ViewBag.InComeParts = new SelectList(db.PARTS.Where(c => c.IsDeleted == false).ToList(), "ID", "PNo");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(WrhsIncomeViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    // Yangi PART_WRHS_INCOMES yozuvini yaratish
                    PART_WRHS_INCOMES newIncome = new PART_WRHS_INCOMES
                    {
                        DocNo = model.DocNo,
                        WHID = model.WHID,
                        InvoiceID = model.InvoiceID,
                        WaybillID = model.WaybillID,
                        Amount = model.Amount,
                        Currency = model.Currency,
                        TotalPrice = model.TotalPrice,
                        IsDeleted = false,
                        Description = model.Description,
                        SenderWHID = model.SenderWHID,
                        IssueDateTime = model.IssueDateTime,
                        RecieveStatus = model.RecieveStatus,
                    };

                    db.PART_WRHS_INCOMES.Add(newIncome);
                    db.SaveChanges();

                    // Yangi yozuvning IncomeID sini olish
                    int newIncomeID = newIncome.ID;

                    // Parts ni saqlash
                    foreach (var part in model.Parts)
                    {
                        PART_WRHS_INCOME_PARTS newPart = new PART_WRHS_INCOME_PARTS
                        {
                            IncomeID = newIncomeID, // part.IncomeID emas, yangi yaratilgan IncomeID ishlatiladi
                            PartID = part.PartID,
                            Unit = part.Unit,
                            Amount = part.Amount,
                            PiecePrice = part.PiecePrice,
                            TotalPrice = part.TotalPrice,
                            Comment = part.Comment
                        };

                        db.PART_WRHS_INCOME_PARTS.Add(newPart);
                    }

                    db.SaveChanges(); // db.SaveChanges() ni bu yerda chaqirish zarur
                    return RedirectToAction("Index");
                }
            }

            // Ma'lumotlar to'g'ri kiritilmagan bo'lsa, view ni qayta yuklash
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Wrhs = new SelectList(db.PART_WRHS.Where(w => w.IsDeleted == false).ToList(), "ID", "WHName", model.WHID);
                ViewBag.Invoices = new SelectList(db.P_INVOICES.Where(i => i.IsDeleted == false).ToList(), "ID", "InvoiceNo", model.InvoiceID);
                ViewBag.Waybills = new SelectList(db.F_WAYBILLS.Where(w => w.IsDeleted == false).ToList(), "ID", "WaybillNo", model.WaybillID);
                ViewBag.InComes = new SelectList(db.PART_WRHS_INCOMES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
                ViewBag.InComeParts = new SelectList(db.PARTS.Where(c => c.IsDeleted == false).ToList(), "ID", "PNo");
            }

            return View(model);
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
            // Process uploaded file
            // ...
            return View();
        }

        public class RepeaterItem
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public int WrhsID { get; set; }
        }
    }
}
