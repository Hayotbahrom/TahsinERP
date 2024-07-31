using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class SOrderController : Controller
    {
        // GET: SOrder
        public ActionResult Index()
        {
            return View();
        }

        // GET: Create
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Customers = new SelectList(db.CUSTOMERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.SContract = new SelectList(db.S_CONTRACTS.Where(x => x.IsDeleted == false).ToList(), "ID", "ContractNo");
                ViewBag.Units = new SelectList(db.UNITS.ToList(), "ID", "ShortName");
                ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(x => x.IsDeleted == false).ToList(), "ID", "PNo");
            }

            return View();
        }
    }
}