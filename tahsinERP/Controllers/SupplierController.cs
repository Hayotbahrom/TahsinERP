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

        public ActionResult Create()
        {
            ViewBag.RoleID = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "True", Value = "true" },
                new SelectListItem { Text = "False", Value = "false" }
            }, "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SUPPLIERS supplier)
        {
            
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }
    }
}