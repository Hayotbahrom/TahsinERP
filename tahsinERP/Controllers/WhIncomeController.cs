using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
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
                ViewBag.InComes = new SelectList(db.PART_WRHS_INCOMES.Where(wi => wi.IsDeleted == false).ToList(), "ID", "DocNo");
                ViewBag.InComeParts = new SelectList(db.PART_WRHS_INCOMES.ToList(), "ID", "IncomeID");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(WhrsIncome model)
        {
            if (ModelState.IsValid)
            {
                // Process the model and save to database
                // Save WhrsIncome
                // Save WhrsIncomePart for each item in model.Parts

                // Sample saving logic (replace with your actual logic)
                foreach (var part in model.Parts)
                {
                    // Save each part
                }

                // Redirect to Index or any other action after successful save
                return RedirectToAction("Index");
            }

            return View(model);


            //var emails = form.GetValues("GroupA[0].Email");
            //var passwords = form.GetValues("GroupA[0].Password");
            //var genders = form.GetValues("GroupA[0].Gender");
            //var professions = form.GetValues("GroupA[0].Profession");

            //var repeaterItems = new List<RepeaterItem>();

            //if (emails != null && passwords != null && genders != null && professions != null)
            //{
            //    for (int i = 0; i < emails.Length; i++)
            //    {
            //        repeaterItems.Add(new RepeaterItem
            //        {
            //            Email = emails[i],
            //            Password = passwords[i],
            //            Gender = genders[i],
            //            Profession = professions[i]
            //        });
            //    }
            //}

            //return View();
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
