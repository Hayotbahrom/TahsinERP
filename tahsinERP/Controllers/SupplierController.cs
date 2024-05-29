using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class SupplierController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private string[] sources = new string[2] { "Import","Lokal" };
        // GET: Supplier
        public ActionResult Index(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                List<SUPPLIERS> list = db.SUPPLIERS.Where(s => s.IsDeleted == false && s.Type.CompareTo(source) == 0).ToList();
                ViewBag.SourceList = new SelectList(sources);
                return View(list);
            }
            else
            {
                List<SUPPLIERS> list = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
                ViewBag.SourceList = new SelectList(sources);
                return View(list);
            }
        }
    }
}