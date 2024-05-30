using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
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
            ViewBag.SourceList = new SelectList(sources);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SUPPLIERS supplier)
        {
            try
            {
                SUPPLIERS new_supplier = new SUPPLIERS();
                new_supplier.Name = supplier.Name;
                new_supplier.Type = "Lokal";
                new_supplier.Country = supplier.Country;
                new_supplier.City = supplier.City;
                new_supplier.Address = supplier.Address;
                new_supplier.Telephone = supplier.Telephone;
                new_supplier.Email = supplier.Email;
                new_supplier.ContactPersonName = supplier.ContactPersonName;
                new_supplier.DirectorName = supplier.DirectorName;
                new_supplier.DUNS = supplier.DUNS;
                new_supplier.IsDeleted = false;

                db.SUPPLIERS.Add(new_supplier);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }
            return RedirectToAction("Index");
        }

        public  ActionResult Details(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var supplier = db.SUPPLIERS.Find(Id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            SUPPLIERS suppliers = new SUPPLIERS();

            suppliers.Name = supplier.Name;
            suppliers.DUNS = supplier.DUNS;
            suppliers.Type = supplier.Type;
            suppliers.Address = supplier.Address;
            suppliers.Country = supplier.Country;
            suppliers.Telephone = supplier.Telephone;
            suppliers.City  = supplier.City;
            suppliers.ContactPersonName = supplier.ContactPersonName;
            suppliers.DirectorName = supplier.DirectorName;
            suppliers.Email = supplier.Email;

            return View(suppliers);
        }
        
    }
}