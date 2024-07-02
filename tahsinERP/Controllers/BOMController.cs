using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class BOMController : Controller
    {

        //private DBTHSNEntities db = new DBTHSNEntities();
        // GET: BOM
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                List<PRODUCT> list = db.PRODUCTS.Where(p => !p.IsDeleted && db.BOMS.Any(b => b.ParentPNo == p.PNo && b.IsParentProduct)).ToList();

                return View(list);
            }
        }
        public ActionResult Details(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PRODUCT product = db.PRODUCTS.Where(p => p.ID == id && p.IsDeleted == false).FirstOrDefault();
                if (product != null)
                {
                    ViewBag.bomList = db.BOMS.Where(b => b.ParentPNo.CompareTo(product.PNo) == 0 && b.IsDeleted == false && b.IsActive == true).ToList();

                    return View(product);
                }
                else
                    return View();
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Processni viewBagga olish
                var process = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Process = new MultiSelectList(process, "ID", "ProcessName");

                // Productni ViewBagga olish
                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "PNo"); // Use the correct property name here

                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");

                return View();
            }
        }
        [HttpPost]
        public ActionResult Create(BOMCreateViewModel bom, int[] processID)
        {

            List<string> processes = new List<string>();

            //foreach (var process in bom.PRODUCTIONPROCESS)
            //{
            //    processes.Add(process.ProcessName);
            //}
            //ViewBag.ProcessList = processes;
            int count = 0;
            foreach (var process in processID)
            {
                count += 1;
            }

            ViewBag.processcount = count;
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var process = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Process = new MultiSelectList(process, "ID", "ProcessName",processID);

                // Productni ViewBagga olish
                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "PNo"); // Use the correct property name here

                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");
            }
            return View(bom);
        }

    }
}