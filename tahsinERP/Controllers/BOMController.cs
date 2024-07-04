using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.Services.Description;
using System.Web.UI.WebControls.WebParts;
using tahsinERP.Models;
using tahsinERP.ViewModels;
using tahsinERP.ViewModels.BOM;

namespace tahsinERP.Controllers
{
    public class BOMController : Controller
    {
        // GET: BOM
        public ActionResult Index(bool? IsParentProduct)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (IsParentProduct == true)
                {
                    //var results = db.BOMS.Where(bom => bom.IsParentProduct == true).GroupBy(bom => bom.ParentPNo).Select(group => group.FirstOrDefault()).ToList();
                    List<IndexViewModel> result = db.BOMS.Join(db.PRODUCTS, bom => bom.ParentPNo, product => product.PNo, (bom, product) =>
                    new IndexViewModel
                    {
                        ParentPNo = bom.ParentPNo,
                        ID = product.ID
                    })
                        .GroupBy(item => new { item.ParentPNo, item.ID })
                        .Select(group => group.FirstOrDefault()).ToList();
                    ViewBag.IsParentProduct = true;
                    return View(result);
                }
                else
                {
                    List<IndexViewModel> result = db.BOMS.Join(db.PARTS, bom => bom.ParentPNo, product => product.PNo, (bom, product) =>
                   new IndexViewModel
                   {
                       ParentPNo = bom.ParentPNo,
                       ID = product.ID
                   })
                       .GroupBy(item => new { item.ParentPNo, item.ID })
                       .Select(group => group.FirstOrDefault()).ToList();

                    ViewBag.IsParentProduct = false;
                    return View(result);
                }
            }
        }
        public ActionResult Details(int ID, bool isParent)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PRODUCT product = db.PRODUCTS.Where(p => p.ID == ID && p.IsDeleted == false).FirstOrDefault();
                PART part = db.PARTS.Where(p => p.ID == ID && p.IsDeleted == false).FirstOrDefault();
                
                if (isParent)
                {
                    var bomList = db.BOMS.Where(b => b.ParentPNo == product.PNo && b.IsDeleted == false).ToList();

                    ParentViewModel parent = new ParentViewModel();
                    parent.PRODUCT = product;
                    parent.ParentPNo = product.PNo;
                    parent.IsParentProduct = true;
                    parent.ParentImageBase64 = GetParentImage(product.ID, parent.IsParentProduct);

                    if (bomList.Count > 0)
                    {
                        foreach (var bom in bomList)
                        {
                            PART childPart = db.PARTS.Where(p => p.PNo == bom.ChildPNo && p.IsDeleted == false).FirstOrDefault();
                            if (childPart != null)
                            {
                                ChildViewModel child = new ChildViewModel();
                                child.PART = childPart;
                                child.ChildPNo = childPart.PNo;
                                child.ChildImageBase64 = GetChildImage(childPart.ID);
                                child.Consumption = bom.Consumption;
                                child.ConsumptionUnit = bom.ConsumptionUnit;
                                parent.Children.Add(child);

                                if (CheckForParentStatusOfChild(childPart.PNo))
                                {
                                    var childBomList = db.BOMS.Where(b => b.ParentPNo == child.PART.PNo && b.IsDeleted == false).ToList();
                                    foreach (var childBom in childBomList)
                                    {
                                        PART grandchildPart = db.PARTS.Where(p => p.PNo == childBom.ChildPNo && p.IsDeleted == false).FirstOrDefault();
                                        if (grandchildPart != null)
                                        {
                                            ChildViewModel grandChild = new ChildViewModel();
                                            grandChild.PART = grandchildPart;
                                            grandChild.ChildPNo = grandchildPart.PNo;
                                            grandChild.ChildImageBase64 = GetChildImage(grandchildPart.ID);
                                            grandChild.Consumption = childBom.Consumption;
                                            grandChild.ConsumptionUnit = childBom.ConsumptionUnit;
                                            child.Children.Add(grandChild);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return View(parent);
                }
                else if (product == null && part == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    var bomList = db.BOMS.Where(b => b.ParentPNo == part.PNo && b.IsDeleted == false).ToList();

                    ParentViewModel parent = new ParentViewModel();
                    parent.PART = part;
                    parent.ParentPNo = part.PNo;
                    parent.IsParentProduct = false;
                    parent.ParentImageBase64 = GetParentImage(part.ID, parent.IsParentProduct);

                    if (bomList.Count > 0)
                    {
                        foreach (var bom in bomList)
                        {
                            PART childPart = db.PARTS.Where(p => p.PNo == bom.ChildPNo && p.IsDeleted == false).FirstOrDefault();
                            if (childPart != null)
                            {
                                ChildViewModel child = new ChildViewModel();
                                child.PART = childPart;
                                child.ChildPNo = childPart.PNo;
                                child.ChildImageBase64 = GetChildImage(childPart.ID);
                                child.Consumption = bom.Consumption;
                                child.ConsumptionUnit = bom.ConsumptionUnit;
                                parent.Children.Add(child);

                                //if (CheckForParentStatusOfChild(childPart.PNo))
                                //{
                                //    var childBomList = db.BOMS.Where(b => b.ParentPNo == child.PART.PNo && b.IsDeleted == false).ToList();
                                //    foreach (var childBom in childBomList)
                                //    {
                                //        PART grandchildPart = db.PARTS.Where(p => p.PNo == childBom.ChildPNo && p.IsDeleted == false).FirstOrDefault();
                                //        if (grandchildPart != null)
                                //        {
                                //            ChildViewModel grandChild = new ChildViewModel();
                                //            grandChild.PART = grandchildPart;
                                //            grandChild.ChildPNo = grandchildPart.PNo;
                                //            grandChild.ChildImageBase64 = GetChildImage(grandchildPart.ID);
                                //            grandChild.Consumption = childBom.Consumption;
                                //            grandChild.ConsumptionUnit = childBom.ConsumptionUnit;
                                //            child.Children.Add(grandChild);
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                    return View(parent);
                }
            }
        }
        private bool CheckForParentStatusOfChild(string pNo)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var bom = db.BOMS.Where(b => b.ParentPNo.CompareTo(pNo) == 0 && b.IsDeleted == false).FirstOrDefault();
                if (bom != null)
                    return true;
                else
                    return false;
            }
        }
        private string GetParentImage(int iD, bool isParentProduct)
        {
            string Base64String = "";
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (isParentProduct)
                {
                    var productImage = db.PRODUCTIMAGES.FirstOrDefault(pi => pi.ProdID == iD);
                    if (productImage != null)
                    {
                        Base64String = "data:image/jpeg;base64," + Convert.ToBase64String(productImage.Image);
                    }
                }
                else
                {
                    var partImage = db.PARTIMAGES.FirstOrDefault(pi => pi.PartID == iD);
                    if (partImage != null)
                    {
                        Base64String = "data:image/jpeg;base64," + Convert.ToBase64String(partImage.Image);
                    }
                }
                return Base64String;
            }
        }
        private string GetChildImage(int iD)
        {
            string Base64String = "";
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var partImage = db.PARTIMAGES.FirstOrDefault(pi => pi.PartID == iD);
                if (partImage != null)
                {
                    Base64String = "data:image/jpeg;base64," + Convert.ToBase64String(partImage.Image);
                }
                return Base64String;
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