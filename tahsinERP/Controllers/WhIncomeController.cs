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
    public class WhIncomeController : Controller
    {
        private string[] sources = ConfigurationManager.AppSettings["partTypes"].Split(',');
        // GET: WhIncome
        public ActionResult Index(string type, int? supplierID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (!string.IsNullOrEmpty(type))
                {
                    if (supplierID.HasValue)
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0 && pi.P_INVOICES.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SUPPLIER.Type.CompareTo(type) == 0).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.Type.CompareTo(type) == 0 && s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
                else
                {
                    if (supplierID.HasValue)
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Where(pi => pi.IsDeleted == false && pi.P_INVOICES.SupplierID == supplierID).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name", supplierID);
                        return View(list);
                    }
                    else
                    {
                        List<P_WRHS_INCOMES> list = db.P_WRHS_INCOMES.Where(pi => pi.IsDeleted == false).ToList();
                        ViewBag.SourceList = new SelectList(sources, type);
                        ViewBag.SupplierList = new SelectList(db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList(), "ID", "Name");
                        return View(list);
                    }
                }
            }
        }
        public ActionResult Create()
        {
            return View();
        }
    }
}