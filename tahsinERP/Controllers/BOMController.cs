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
        public ActionResult Index(IndexViewModel viewModel)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (viewModel.IsParentProduct == true)
                {
                    //var results = db.BOMS.Where(bom => bom.IsParentProduct == true).GroupBy(bom => bom.ParentPNo).Select(group => group.FirstOrDefault()).ToList();
                    List<IndexViewModel> result = db.BOMS.Join(db.PRODUCTS, bom => bom.ParentPNo, product => product.PNo, (bom, product) =>
                    new IndexViewModel
                    {
                        ParentPNo = bom.ParentPNo,
                        ProductName = product.Name,
                        ProductID = product.ID,
                        IsParentProduct = true,
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
                       ProductName = product.PName,
                       ProductID = product.ID,
                       IsParentProduct = false,
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
        public ActionResult WizardView()
        {
            return View();
        }
        [HttpPost]
        public ActionResult WizardView(BomViewModel model)
        {
            return View();
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

                return View(new BomViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BomViewModel model, int[] processID)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var selectedProcesses = db.PRODUCTIONPROCESSES
                                              .Where(x => processID.Contains(x.ID) && x.IsDeleted == false)
                                              .ToList();

                    model.Process = string.Join(", ", selectedProcesses.Select(p => p.ProcessName));
                    model.SelectedProcessIds = processID;  // Add this line to ensure the process IDs are passed along
                    var product = db.PRODUCTS.FirstOrDefault(x => x.ID == model.ProductID && x.IsDeleted == false);
                    model.Product = product;
                    model.ProductNo = product.PNo;
                }
                return RedirectToAction("CreateWizard", model);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var process = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Process = new MultiSelectList(process, "ID", "ProcessName");

                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "PNo");

                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");
            }

            return View(model);
        }

        public ActionResult CreateWizard(BomViewModel model)
        {
            if (model == null)
                return RedirectToAction("Create");
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var createViewModel = new BOMCreateViewModel();
                var product = db.PRODUCTS.FirstOrDefault(x => x.ID == model.ProductID && x.IsDeleted == false);
                createViewModel.Product = product;
                createViewModel.ProductID = model.ProductID;
                createViewModel.ProductNo = model.ProductNo;
                createViewModel.SelectedProcessIds = model.SelectedProcessIds;
                createViewModel.Process = model.Process;
                createViewModel.IsActive = model.IsActive;



                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");

                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "PNo");

                var slittingNorms = db.SLITTING_NORMS
                                      .Where(x => x.IsDeleted == false)
                                      .Select(x => new
                                      {
                                          x.ID,
                                          PartInfo = db.PARTS.Where(p => p.ID == x.PartID_after).Select(p => p.PNo).FirstOrDefault() + " - " +
                                                     db.PARTS.Where(p => p.ID == x.PartID_before).Select(p => p.PNo).FirstOrDefault()
                                      })
                                      .ToList();

                ViewBag.SlittingNorms = new SelectList(slittingNorms, "ID", "PartInfo");

                var blankingNorms = db.BLANKING_NORMS
                                      .Where(x => x.IsDeleted == false)
                                      .Select(x => new
                                      {
                                          x.ID,
                                          PartInfo = db.PARTS.Where(p => p.ID == x.PartID_after).Select(p => p.PNo).FirstOrDefault() + " - " +
                                                     db.PARTS.Where(p => p.ID == x.PartID_before).Select(p => p.PNo).FirstOrDefault()
                                      })
                                      .ToList();

                ViewBag.BlankingNorms = new SelectList(blankingNorms, "ID", "PartInfo");

                var stamping = db.STAMPING_NORMS
                                      .Where(x => x.IsDeleted == false)
                                      .Select(x => new
                                      {
                                          x.ID,
                                          PartInfo = db.PARTS.Where(p => p.ID == x.PartID_after).Select(p => p.PNo).FirstOrDefault() + " - " +
                                                     db.PARTS.Where(p => p.ID == x.PartID_before).Select(p => p.PNo).FirstOrDefault()
                                      })
                                      .ToList();

                ViewBag.StampingNorms = new SelectList(stamping, "ID", "PartInfo");


                return View(createViewModel);
            }
        }
        private int? GetUserID(string email)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                USER currentUser = db.USERS
                                     .Where(u => u.Email.CompareTo(email) == 0 && u.IsDeleted == false && u.IsActive == true)
                                     .FirstOrDefault();

                return currentUser?.ID;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateWizard(BOMCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    int sequence = 0;
                    var processNames = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false).ToList();
                    var userId = GetUserID(User.Identity.Name);

                    var welding = model.WeldingPart;
                    if (model.SelectedSlittingNormID != 0)
                    {
                        var selectedSlittingNorm = db.SLITTING_NORMS.Find(model.SelectedSlittingNormID);

                        var after_part = db.PARTS.Where(x => x.IsDeleted == false && x.ID == selectedSlittingNorm.PartID_after).FirstOrDefault();
                        var part_before1 = db.PARTS.Where(x => x.IsDeleted == false && x.ID == selectedSlittingNorm.PartID_before).FirstOrDefault();

                        var cutterLines1 = (int)(after_part.PWidth) / ((part_before1.PWidth) - 1);
                        var cutterWidth1 = selectedSlittingNorm.CutterWidth;

                        var bom = new BOM();

                        bom.ChildPNo = part_before1.PNo;
                        bom.ParentPNo = after_part.PNo;
                        bom.IsDeleted = false;
                        if (model.ProductNo != null) { bom.IsParentProduct = true; }
                        else { bom.IsParentProduct = false; }
                        bom.IsActive = true;
                        bom.WasteAmount = (part_before1.PWeight / part_before1.PWidth * cutterLines1 * cutterWidth1);
                        bom.ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Slitting")?.ID;
                        bom.Consumption = selectedSlittingNorm.WeightOfSlittedParts / cutterLines1;
                        bom.ConsumptionUnit = "kg";
                        bom.Sequence = sequence + 1;
                        db.BOMS.Add(bom);
                    }
                    if (model.SLITTING_NORMS != null)
                    {
                        var part_before = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.SLITTING_NORMS.PartID_before);
                        var part_after = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.SLITTING_NORMS.PartID_after);

                        var cutterWidth = model.SLITTING_NORMS.CutterWidth;
                        var pieceCount = (part_before.PWidth / part_after.PWidth);
                        var cutterLines = (pieceCount - 1);

                        if (part_after != null && part_before != null)
                        {
                            var slitting_process = new SLITTING_NORMS
                            {
                                IsDeleted = false,
                                IsActive = model.IsActive,
                                PartID_after = model.SLITTING_NORMS.PartID_after,
                                PartID_before = model.SLITTING_NORMS.PartID_before,
                                SlittingPieces = (int)pieceCount,
                                CutterLines = (int)cutterLines,
                                CutterWidth = cutterWidth,
                                WeightOfSlittedParts = Math.Round((part_after.PWidth * (part_before.PWeight / part_before.PWidth)), 2, MidpointRounding.ToEven),
                                WeightOfCutWaste = Math.Round(((part_before.PWeight / part_before.PWidth) * cutterLines * cutterWidth), 2, MidpointRounding.ToEven),
                                WidthOfUsefulWaste = Math.Round((part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines * cutterWidth)), 2, MidpointRounding.ToEven),
                                WeightOfUsefulWaste = Math.Round(((part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines * cutterWidth)) * (part_before.PWeight / part_before.PWidth)), 2, MidpointRounding.ToEven),
                                IssuedDateTime = DateTime.Now,
                                IssuedByUserID = userId.GetValueOrDefault()
                            };
                            db.SLITTING_NORMS.Add(slitting_process);

                            if (part_after != null)
                            {
                                var bom = new BOM
                                {
                                    ChildPNo = part_before.PNo,
                                    ParentPNo = part_after.PNo,
                                    IsDeleted = false,
                                    IsActive = true,
                                    WasteAmount = Math.Round((part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth), 2, MidpointRounding.ToEven),
                                    ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Slitting")?.ID,
                                    Consumption = Math.Round((part_after.PWidth * (part_before.PWeight / part_before.PWidth) / cutterLines), 2, MidpointRounding.ToEven),
                                    ConsumptionUnit = "kg",
                                    Sequence = sequence + 1,
                                };
                                db.BOMS.Add(bom);
                            }
                        }
                    }

                    if (model.BLANKING_NORMS != null || model.SelectedBlankingNormID != 0)
                    {
                        if (model.BLANKING_NORMS != null)
                        {
                            var part_after_slitting = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.SLITTING_NORMS.PartID_after);
                            var part_after_Blanking = db.PARTS.FirstOrDefault(p => p.IsDeleted == false && p.ID == model.BLANKING_NORMS.PartID_after);
                            if (part_after_Blanking != null && part_after_slitting != null)
                            {
                                var blanking_norms = new BLANKING_NORMS
                                {
                                    IsDeleted = false,
                                    IsActive = model.BLANKING_NORMS.IsActive,
                                    PartID_before = part_after_slitting.ID,
                                    PartID_after = part_after_Blanking.ID,
                                    Density = model.BLANKING_NORMS.Density,
                                    QuantityOfBlanks = (int)(part_after_slitting.PWeight / part_after_Blanking.PWeight),
                                    WeightOfBlanks = (int)(part_after_slitting.PWidth * part_after_slitting.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density),
                                    WeightOfCutWaste = part_after_slitting.PWeight - (part_after_slitting.PWeight * part_after_Blanking.PWeight),
                                    IssuedDateTime = DateTime.Now,
                                    IssuedByUserID = userId.GetValueOrDefault()
                                };
                                db.BLANKING_NORMS.Add(blanking_norms);

                                var bom = new BOM
                                {
                                    ChildPNo = part_after_Blanking.PNo,
                                    ParentPNo = part_after_slitting.PNo,
                                    IsDeleted = false,
                                    IsActive = true,
                                    ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Blanking")?.ID,
                                    WasteAmount = Math.Round((part_after_slitting.PWeight / part_after_slitting.PWidth), 2, MidpointRounding.ToEven),
                                    Consumption = Math.Round((int)(part_after_slitting.PWidth * part_after_slitting.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density) / (part_after_slitting.PWeight / part_after_Blanking.PWeight), 2, MidpointRounding.ToEven),
                                    ConsumptionUnit = "kg",
                                    Sequence = sequence + 2,
                                };
                                db.BOMS.Add(bom);
                            }
                            if (model.STAMPING_NORMS != null || model.SelectedStampingNormID != 0)
                            {

                                var part_after_Stamping = db.PARTS.FirstOrDefault(x => x.ID == model.STAMPING_NORMS.PartID_after);
                                if (model.STAMPING_NORMS != null)
                                {

                                    var stamping = new STAMPING_NORMS
                                    {
                                        IsDeleted = false,
                                        IsActive = model.STAMPING_NORMS.IsActive,
                                        PartID_before = part_after_Blanking.ID,
                                        PartID_after = part_after_Stamping.ID,
                                        Density = model.STAMPING_NORMS.Density,
                                        QuantityOfStamps = (int)(Math.Round((part_after_Blanking.PWeight / part_after_Stamping.PWeight), 2, MidpointRounding.ToEven)),
                                        WeightOfStamps = (Math.Round(part_after_Blanking.PWidth * part_after_Blanking.PLength * part_after_Stamping.Gauge * model.STAMPING_NORMS.Density)),
                                        WeightOfWaste = (part_after_Blanking.PWeight - (part_after_Blanking.PWeight * part_after_Stamping.PWeight)),
                                        IssuedDateTime = DateTime.Now,
                                        IssuedByUserID = userId.GetValueOrDefault()
                                    };
                                    db.STAMPING_NORMS.Add(stamping);

                                    var bom = new BOM
                                    {
                                        ChildPNo = part_after_Blanking.PNo,
                                        ParentPNo = part_after_Stamping.PNo,
                                        IsDeleted = false,
                                        IsActive = true,
                                        ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Stamping")?.ID,
                                        WasteAmount = Math.Round((part_after_Stamping.PWeight / part_after_Stamping.PWidth), 2, MidpointRounding.ToEven),
                                        Consumption = (Math.Round(part_after_Blanking.PWidth * part_after_Blanking.PLength * part_after_Stamping.Gauge * model.STAMPING_NORMS.Density) / (part_after_Blanking.PWeight / part_after_Stamping.PWeight)),
                                        ConsumptionUnit = "kg",
                                        Sequence = sequence + 3,
                                    };
                                    db.BOMS.Add(bom);

                                }
                                else if (model.SelectedStampingNormID != 0)
                                {
                                    var selectedStampingNorm = db.STAMPING_NORMS.Find(model.SelectedStampingNormID);
                                    if (selectedStampingNorm != null)
                                    {
                                        var part_after_stamping = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == selectedStampingNorm.PartID_after);

                                        var bom = new BOM
                                        {
                                            ChildPNo = part_after_Stamping.PNo,
                                            ParentPNo = part_after_Blanking.PNo,
                                            IsDeleted = false,
                                            IsActive = true,
                                            ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Blanking")?.ID,
                                            Consumption = (int)(part_after_slitting.PWidth * part_after_slitting.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density) / (part_after_slitting.PWeight / part_after_Blanking.PWeight),
                                            ConsumptionUnit = "kg",
                                            Sequence = sequence + 1,
                                        };
                                        db.BOMS.Add(bom);
                                    }
                                }
                            }
                        }
                        else if (model.SelectedBlankingNormID != 0)
                        {
                            var selectedBlankingNorm = db.BLANKING_NORMS.Find(model.SelectedBlankingNormID);
                            if (selectedBlankingNorm != null)
                            {
                                if (model.SelectedBlankingNormID != 0)
                                {
                                    var part_before_Blanking = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == selectedBlankingNorm.PartID_after);
                                    var part_after_Blanking = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == selectedBlankingNorm.PartID_before);

                                    if (part_after_Blanking != null && part_before_Blanking != null)
                                    {
                                        var bom = new BOM
                                        {
                                            ChildPNo = part_after_Blanking.PNo,
                                            ParentPNo = part_before_Blanking.PNo,
                                            IsDeleted = false,
                                            IsActive = true,
                                            ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Blanking")?.ID,
                                            Consumption = (int)(part_before_Blanking.PWidth * part_before_Blanking.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density) / (part_before_Blanking.PWeight / part_after_Blanking.PWeight),
                                            ConsumptionUnit = "kg",
                                            Sequence = sequence + 2,
                                        };
                                        db.BOMS.Add(bom);
                                    }
                                }
                                if (model.STAMPING_NORMS != null)
                                {
                                    var part_after_Blanking = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == selectedBlankingNorm.PartID_before);
                                    var part_after_Stamping = db.PARTS.FirstOrDefault(x => x.ID == model.SelectedStampingNormID);
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
                                            IssuedByUserID = userId.GetValueOrDefault()
                                        };
                                        db.STAMPING_NORMS.Add(stamping);

                                        var bom = new BOM
                                        {
                                            ChildPNo = part_after_Blanking.PNo,
                                            ParentPNo = part_after_Stamping.PNo,
                                            IsDeleted = false,
                                            IsActive = true,
                                            ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Stamping")?.ID,
                                            Consumption = (int)(part_after_Blanking.PWidth * part_after_Blanking.PLength * part_after_Stamping.Gauge * model.STAMPING_NORMS.Density) / (part_after_Blanking.PWeight / part_after_Stamping.PWeight),
                                            ConsumptionUnit = "kg",
                                            Sequence = sequence + 3,
                                        };
                                        db.BOMS.Add(bom);
                                    }
                                    else if (model.SelectedStampingNormID != 0)
                                    {
                                        var selectedStampingNorm = db.STAMPING_NORMS.Find(model.SelectedStampingNormID);
                                        var part_before_stamping = db.PARTS.Where(x => x.ID == selectedStampingNorm.PartID_before && x.IsDeleted == false).FirstOrDefault();
                                        var part_after_stamping = db.PARTS.Where(p => p.ID == selectedStampingNorm.PartID_after && p.IsDeleted == false).FirstOrDefault();



                                        var bom = new BOM
                                        {
                                            ChildPNo = part_after_Stamping.PNo,
                                            ParentPNo = part_before_stamping.PNo,
                                            IsDeleted = false,
                                            IsActive = true,
                                            ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Blanking")?.ID,
                                            Consumption = (int)(part_before_stamping.PWidth * part_before_stamping.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density) / (part_before_stamping.PWeight / part_after_Blanking.PWeight),
                                            ConsumptionUnit = "kg",
                                            Sequence = sequence + 3,
                                        };
                                        db.BOMS.Add(bom);

                                    }
                                }
                            }
                        }

                    }
                    if (model.WeldingPart != null)
                    {
                        foreach (var part in model.WeldingPart)
                        {
                            var unit_part = db.PARTS.FirstOrDefault(x => x.ID == part.Welding_PartID);
                            var bom = new BOM
                            {
                                ChildPNo = unit_part.PNo,
                                ParentPNo = model.ProductNo,
                                IsDeleted = false,
                                IsActive = true,
                                ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Welding")?.ID,
                                ConsumptionUnit = "kg",
                                Consumption = part.WeldingQuantity,
                                Sequence = sequence + 4,
                            };
                            db.BOMS.Add(bom);
                        }

                    }

                    if (model.AssemblyPart != null)
                    {
                        int count = 0;
                        foreach (var part in model.AssemblyPart)
                        {
                            var bom = new BOM();
                            bom.ChildPNo = part.PNo;
                            bom.ParentPNo = model.ProductNo;
                            bom.IsDeleted = false;
                            bom.IsActive = true;
                            bom.ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Assembly")?.ID;
                            bom.ConsumptionUnit = "kg";
                            bom.Sequence = sequence + 5;
                            count += 1;
                            bom.Consumption = count;

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

                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "PNo");
            }

            return View(model);
        }
    }
}