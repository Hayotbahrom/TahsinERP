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
            sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
            sources = sources.Where(x => !x.Equals("InHouse", StringComparison.OrdinalIgnoreCase)).ToArray();
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