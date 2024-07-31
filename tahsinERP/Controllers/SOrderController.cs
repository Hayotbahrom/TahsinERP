using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class SOrderController : Controller
    {
        private string supplierName, contractNo, orderNo, partNo = "";
        private string[] sources;
        public SOrderController()
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

        // GET: SOrder
        //public ActionResult Index(string type, int? supplierID)
        //{
        //    using (DBTHSNEntities db = new DBTHSNEntities())
        //    {
        //        var suppliers = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
        //        if (!string.IsNullOrEmpty(type))
        //        {
        //            if (supplierID.HasValue)
        //            {
        //                ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false && s.SupplierID == supplierID && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
        //                ViewBag.SourceList = new SelectList(sources, type);
        //                ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.Type.CompareTo(type) == 0), "ID", "Name", supplierID);
        //            }
        //            else
        //            {
        //                ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false && (s.SUPPLIER.Type.CompareTo(type) == 0)).ToList();
        //                ViewBag.SourceList = new SelectList(sources, type);
        //                ViewBag.SupplierList = new SelectList(suppliers.Where(x => x.Type.CompareTo(type) == 0), "ID", "Name");
        //            }
        //        }
        //        else
        //        {
        //            if (supplierID.HasValue)
        //            {
        //                ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false && s.SupplierID == supplierID).ToList();
        //                ViewBag.SourceList = new SelectList(sources, type);
        //                ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name", supplierID);
        //            }
        //            else
        //            {
        //                ViewBag.partList = db.P_ORDERS.Include(x => x.P_CONTRACTS).Where(s => s.IsDeleted == false).ToList();
        //                ViewBag.SourceList = new SelectList(sources);
        //                ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name");
        //            }
        //        }
        //        return View();
        //    }
        //}

        // GET: Creste

        // POST: Create

        // GET: Edit

        // POST: Edit

        // GET: Delete

        // POST: Delete
    }
}