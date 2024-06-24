using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ProductPackController : Controller
    {
        // GET: ProductPack
        public ActionResult Index()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                var productpack = db.PRODUCTPACKS.Where(x => x.IsDeleted == false).ToList();
                return View(productpack);
            }
        }
    }
}