using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
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

                return View(new BOMCreateViewModel());
            }
        }

        [HttpPost]
        public ActionResult Create(BOMCreateViewModel model, int[] processID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var selectedProcesses = db.PRODUCTIONPROCESSES
                                          .Where(x => processID.Contains(x.ID) && x.IsDeleted == false)
                                          .ToList();

                model.Process = string.Join(", ", selectedProcesses.Select(p => p.ProcessName));

                var product = db.PRODUCTS.FirstOrDefault(x => x.ID == model.ProductID && x.IsDeleted == false);

                model.ProductNo = product.PNo;

                TempData["BOMCreateViewModel"] = model;
                return RedirectToAction("CreateWizard");
            }
        }

        public ActionResult CreateWizard()
        {
            var model = TempData["BOMCreateViewModel"] as BOMCreateViewModel;

            if (model == null)
                return RedirectToAction("Create");

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult CreateWizard(BOMCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var userImageCookie = Request.Cookies["UserImageId"];
                    int userImageId = int.Parse(userImageCookie.Value);

                    var userimage = db.USERIMAGES.FirstOrDefault(x => x.ID == userImageId);
                    int userId = userimage.UserID;

                    var processNames = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false).ToList();

                    var part_before = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.SLITTING_NORMS.PartID_before);
                    var part_after = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.SLITTING_NORMS.PartID_after);

                    var cutterLines = (int)((part_before.PWidth) / (part_after.PWidth) - 1);
                    var cutterWidth = model.SLITTING_NORMS.CutterWidth;
                    var pieceCount = Convert.ToInt32(Math.Round(part_before.PWidth / part_after.PWidth));

                    if (part_after != null && part_before != null)
                    {
                        var slitting_process = new SLITTING_NORMS
                        {
                            IsDeleted = false,
                            IsActive = model.SLITTING_NORMS.IsActive,
                            PartID_after = model.SLITTING_NORMS.PartID_after,
                            PartID_before = model.SLITTING_NORMS.PartID_before,
                            SlittingPieces = pieceCount,
                            CutterLines = cutterLines,
                            CutterWidth = cutterWidth,
                            WeightOfSlittedParts = part_after.PWidth * (part_before.PWeight / part_before.PWidth),
                            WeightOfCutWaste = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth),
                            WidthOfUsefulWaste = part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines - cutterWidth),
                            WeightOfUsefulWaste = (part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines - cutterWidth)) * (part_before.PWeight / part_before.PWidth),
                            IssuedDateTime = DateTime.Now,
                            IssuedByUserID = userId
                        };
                        db.SLITTING_NORMS.Add(slitting_process);

                        if (part_after != null)
                        {
                            var bom = new BOM
                            {
                                ChildPNo = part_after.PNo,
                                ParentPNo = model.ProductNo,
                                IsDeleted = false,
                                IsActive = true,
                                WasteAmount = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth),
                                ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Sliting")?.ID
                            };

                            db.BOMS.Add(bom);
                        }
                    }

                    if (model.BLANKING_NORMS != null)
                    {
                        var part_after_Blanking = db.PARTS.FirstOrDefault(p => p.IsDeleted == false && p.ID == model.BLANKING_NORMS.PartID_after);
                        if (part_after_Blanking != null && part_after != null)
                        {
                            var blanking_norms = new BLANKING_NORMS
                            {
                                IsDeleted = false,
                                IsActive = model.BLANKING_NORMS.IsActive,
                                PartID_before = part_after.ID,
                                PartID_after = part_after_Blanking.ID,
                                Density = model.BLANKING_NORMS.Density,
                                QuantityOfBlanks = (int)(part_after.PWeight / part_after_Blanking.PWeight),
                                WeightOfBlanks = (int)(part_after.PWidth * part_after.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density),
                                WeightOfCutWaste = part_after.PWeight - (part_after.PWeight * part_after_Blanking.PWeight),
                                IssuedDateTime = DateTime.Now,
                                IssuedByUserID = userId
                            };

                            db.BLANKING_NORMS.Add(blanking_norms);

                            var bom = new BOM
                            {
                                ChildPNo = part_after_Blanking.PNo,
                                ParentPNo = model.ProductNo,
                                IsDeleted = false,
                                IsActive = true,
                                WasteAmount = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth),
                                ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Blanking")?.ID
                            };

                            db.BOMS.Add(bom);
                        }

                        var part_after_Stamping = db.PARTS.FirstOrDefault(x => x.ID == model.STAMPING_NORMS.PartID_after);
                        if (part_after_Stamping != null && part_after_Blanking != null)
                        {
                            var stamping = new STAMPING_NORMS
                            {
                                IsDeleted = false,
                                IsActive = model.STAMPING_NORMS.IsActive,
                                PartID_before = part_after_Blanking.ID,
                                PartID_after = part_after_Stamping.ID,
                                Density = model.STAMPING_NORMS.Density,
                                QuantityOfStamps = (int)(part_after_Blanking.PWeight / part_after_Stamping.PWeight),
                                WeightOfStamps = (int)(part_after_Blanking.PWidth * part_after_Blanking.PLength * part_after_Stamping.Gauge * model.STAMPING_NORMS.Density),
                                WeightOfWaste = part_after_Blanking.PWeight - (part_after_Blanking.PWeight * part_after_Stamping.PWeight),
                                IssuedDateTime = DateTime.Now,
                                IssuedByUserID = userId
                            };
                            db.STAMPING_NORMS.Add(stamping);

                            var bom = new BOM
                            {
                                ChildPNo = part_after_Stamping.PNo,
                                ParentPNo = model.ProductNo,
                                IsDeleted = false,
                                IsActive = true,
                                WasteAmount = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth),
                                ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Stamping")?.ID
                            };
                            db.BOMS.Add(bom);
                        }
                    }

                    if (model.WeldingPart != null)
                    {
                        foreach (var part in model.WeldingPart)
                        {
                            var bom = new BOM
                            {
                                ChildPNo = part.PNo,
                                ParentPNo = model.ProductNo,
                                IsDeleted = false,
                                IsActive = true,
                                WasteAmount = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth),
                                ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Welding")?.ID
                            };
                            db.BOMS.Add(bom);
                        }
                    }

                    if (model.AssemblyPart != null)
                    {
                        foreach (var part in model.AssemblyPart)
                        {
                            var bom = new BOM
                            {
                                ChildPNo = part.PNo,
                                ParentPNo = model.ProductNo,
                                IsDeleted = false,
                                IsActive = true,
                                WasteAmount = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth),
                                ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Assembly")?.ID
                            };
                            db.BOMS.Add(bom);
                        }
                    }

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");
            }

            return View(model);
        }

    }
}