using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using tahsinERP.Models;
using tahsinERP.ViewModels;

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
                List<SUPPLIER> list = db.SUPPLIERS.Where(s => s.IsDeleted == false && s.Type.CompareTo(type) == 0).ToList();
                ViewBag.SourceList = new SelectList(sources, type);
                return View(list);
            }
            else
            {
                List<SUPPLIER> list = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
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
        public ActionResult Create([Bind(Include = "Name, DUNS, Type, Country, City, Address, Telephone, E_mail, ContactPerson, Director, IsDeleted")] SUPPLIER supplier)
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
        public ActionResult Details(int? Id)
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
            ViewBag.partList = SupplierParts(supplier.ID);
            return View(supplier);
        }
        public List<GetSupplierParts_Result> SupplierParts(int? supplierId)
        {
            if (supplierId == null)
            {
                // Handle missing SupplierID (e.g., show an error message)
                return null;
            }

            var supplierData = db.Database.SqlQuery<GetSupplierParts_Result>(
                "EXEC GetSupplierParts @SupplierID",
                new SqlParameter("@SupplierID", supplierId)
            ).ToList();

            return supplierData;
        }
        public ActionResult Edit(int? Id)
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
            ViewBag.Type = new SelectList(sources, supplier.Type);
            return View(supplier);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SUPPLIER supplier)
        {
            if (ModelState.IsValid)
            {
                SUPPLIER supplierToUpdate = db.SUPPLIERS.Find(supplier.ID);
                if (supplierToUpdate != null)
                {
                    supplierToUpdate.IsDeleted = false;
                    supplierToUpdate.Type = supplier.Type;
                    if (TryUpdateModel(supplierToUpdate, "", new string[] { "Name", "DUNS", "Type", "Country", "City", "Address", "Telephone", "Email", "ContactPersonName", "DirectorName", "IsDeleted" }))
                    {
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                        }
                    }
                }
                return View(supplierToUpdate);
            }
            return View();
        }
        public ActionResult Delete(int? Id)
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

            return View(supplier);
            //return RedirectToAction("SupplierParts?supplierId="+supplier.ID);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                SUPPLIER supplierToUpdate = db.SUPPLIERS.Find(ID);
                if (supplierToUpdate != null)
                {
                    supplierToUpdate.IsDeleted = true;
                    if (TryUpdateModel(supplierToUpdate, "", new string[] { "IsDeleted" }))
                    {
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                        }
                    }
                }
            }
            return View();
        }
    }
}