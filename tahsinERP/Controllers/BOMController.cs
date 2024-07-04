using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class BOMController : Controller
    {

        //private DBTHSNEntities db = new DBTHSNEntities();
        // GET: BOM
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                List<PRODUCT> list = db.PRODUCTS.Where(p => !p.IsDeleted && db.BOMS.Any(b => b.ParentPNo == p.PNo && b.IsParentProduct)).ToList();

                return View(list);
            }
        }
        public ActionResult Details(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PRODUCT product = db.PRODUCTS.Where(p => p.ID == id && p.IsDeleted == false).FirstOrDefault();
                if (product != null)
                {
                    ViewBag.bomList = db.BOMS.Where(b => b.ParentPNo.CompareTo(product.PNo) == 0 && b.IsDeleted == false && b.IsActive == true).ToList();

                    return View(product);
                }
                else
                    return View();
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var process = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Process = new MultiSelectList(process, "ID", "ProcessName");

                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "PNo");

                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");

                return View();
            }
        }

        [HttpPost]
        public ActionResult Create(BOMCreateViewModel bom, int[] processID)
        {

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var selectedProcesses = db.PRODUCTIONPROCESSES
                                          .Where(x => processID.Contains(x.ID) && x.IsDeleted == false)
                                          .ToList();

                bom.Process = string.Join(", ", selectedProcesses.Select(p => p.ProcessName));

                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "PNo");

                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");

            }

            return RedirectToAction("CreateWizard", bom);
        }

        public ActionResult CreateWizard(BOMCreateViewModel bom)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");
                return View(bom);
            }
        }

        [HttpPost]
        public ActionResult CreateProcess(BOMCreateViewModel model, int[] progresID)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.Process))
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        foreach (var process in progresID)
                        {
                            BOM bom = new BOM();
                            bom.ProcessID = process;
                            bom.ParentPNo = model.product.PNo;
                            bom.ChildPNo = model.part.PNo;
                            db.BOMS.Add(bom);
                        }
                        var processNames = model.Process.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var processName in processNames)
                        {
                            switch (processName)
                            {
                                case "Sliting":
                                    var slitting = new SLITTING_NORMS
                                    {
                                        PartID_before = model.SLITTING_NORMS.PartID_before,
                                        PartID_after = model.SLITTING_NORMS.PartID_after,
                                        CutterWidth = model.SLITTING_NORMS.CutterWidth
                                    };
                                    db.SLITTING_NORMS.Add(slitting);
                                    break;

                                case "Blanking":
                                    var blanking = new BLANKING_NORMS
                                    {
                                        PartID_before = model.BLANKING_NORMS.PartID_before,
                                        PartID_after = model.BLANKING_NORMS.PartID_after
                                    };
                                    db.BLANKING_NORMS.Add(blanking);
                                    break;

                                case "Stamping":
                                    var stamping = new STAMPING_NORMS
                                    {
                                        PartID_before = model.STAMPING_NORMS.PartID_before,
                                        PartID_after = model.STAMPING_NORMS.PartID_after
                                    };
                                    db.STAMPING_NORMS.Add(stamping);
                                    break;

                                    //case "Welding":
                                    //    var welding = new WELDING_NORMS
                                    //    {
                                    //        PartID_before = model.WELDING_NORMS.PartID_before,
                                    //        PartID_after = model.WELDING_NORMS.PartID_after
                                    //    };
                                    //    _context.WELDING_NORMS.Add(welding);
                                    //    break;

                                    //case "Painting":
                                    //    var painting = new PAINTING_NORMS
                                    //    {
                                    //        PartID_before = model.PAINTING_NORMS.PartID_before,
                                    //        PartID_after = model.PAINTING_NORMS.PartID_after
                                    //    };
                                    //    _context.PAINTING_NORMS.Add(painting);
                                    //    break;
                            }
                        }
                        db.SaveChanges();
                        return RedirectToAction("Index"); // Or whatever action you want to redirect to after a successful create
                    }
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }


    }
}