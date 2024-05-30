using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ProductController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        // GET: Product
        public  ActionResult Index()
        {
            var product = db.PRODUCTS.Where(x =>x.IsDeleted == false).ToList();
            return View(product);
        }
    }
}