using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PartController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private string[] sources = new string[3] { "", "Import", "Lokal" };
        // GET: Part
        public ActionResult Index(string type, int? supplierID)
        {
            var suppliers = db.SUPPLIERS.Where(s => s.IsDeleted == false).ToList();
            if (!string.IsNullOrEmpty(type))
            {
                if (supplierID.HasValue)
                {
                    ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_by_type_and_supplierID_Result>("EXEC GetPartsInfo_by_type_and_supplierID @Type, @SupplierID", new SqlParameter("@Type", type), new SqlParameter("@SupplierID", supplierID)).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name", supplierID);
                }
                else
                {
                    ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_by_type_Result>("EXEC GetPartsInfo_by_type @Type", new SqlParameter("@Type", type)).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name");
                }
            }
            else
            {
                if (supplierID.HasValue)
                {
                    ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_by_supplierID_Result>("EXEC GetPartsInfo_by_supplierID @SupplierID", new SqlParameter("@SupplierID", supplierID)).ToList();
                    ViewBag.SourceList = new SelectList(sources, type);
                    ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name", supplierID);
                }
                else
                {
                    ViewBag.partList = db.Database.SqlQuery<GetPartsInfo_Result>("EXEC GetPartsInfo").ToList();
                    ViewBag.SourceList = new SelectList(sources);
                    ViewBag.SupplierList = new SelectList(suppliers, "ID", "Name");
                }
            }
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(PART part)
        {
            return View();
        }
        public ActionResult UploadWithExcel()
        {
            return View();
        }
        public ActionResult Edit()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Edit(PART part)
        {
            return View();
        }
        public ActionResult Delete()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Delete(PART part)
        {
            return View();
        }
        public ActionResult Details(int? ID)
        {
            return View();
        }
    }
}