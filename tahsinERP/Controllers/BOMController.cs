using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}