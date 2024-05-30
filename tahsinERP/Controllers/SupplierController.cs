using System;
using System.Collections.Generic;
using System.Data;
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
        private string[] sources = new string[3] { "", "Import", "Lokal" };
        // GET: Supplier
        public ActionResult Index(string type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                List<SUPPLIERS> list = db.SUPPLIERS.Where(s => s.IsDeleted == false && s.Type.CompareTo(type) == 0).ToList();
                ViewBag.SourceList = new SelectList(sources, type);
                return View(list);
            }
            else
            {
                List<SUPPLIERS> list = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
                ViewBag.SourceList = new SelectList(sources, type);
                return View(list);
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name, DUNS, Type, Country, City, Address, Telephone, E_mail, ContactPerson, Director, IsDeleted")] SUPPLIERS supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    supplier.IsDeleted = false;
                    db.SUPPLIERS.Add(supplier);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }
            return View(supplier);
        }

        public ActionResult Edit()
        {
            return View();
        }
    }
}