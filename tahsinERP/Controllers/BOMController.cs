using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using tahsinERP.Models;
using tahsinERP.ViewModels.BOM;
using static tahsinERP.ViewModels.BOM.BOMCreateProductViewModel;

namespace tahsinERP.Controllers
{
    public class BOMController : Controller
    {
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
                try
                {
                    if (product != null)
                    {
                        var rootItem = GetBomTree(product.PNo);
                        return View(rootItem);
                    }
                    else if (part != null)
                    {
                        var rootItem = GetBomTree(part.PNo);
                        return View(rootItem);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex);
                }
                return View();
            }
        }

        private BoomViewModel GetBomTree(string parentPno)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var parentItems = db.BOMS.Where(b => b.ParentPNo == parentPno && b.IsDeleted == false).ToList();

                if (!parentItems.Any())
                {
                    return null;
                }

                var root = new BoomViewModel()
                {
                    ParentPNo = parentPno,
                    ParentImageBase64 = GetParentImage(parentPno),
                    Children = parentItems.Select(b => new BoomViewModel
                    {
                        ParentPNo = b.ParentPNo,
                        ChildPNo = b.ChildPNo,
                        ChildImageBase64 = GetChildImage(b.ChildPNo),
                        Consumption = b.Consumption,
                        ConsumptionUnit = b.ConsumptionUnit,
                        Children = GetBomTree(b.ChildPNo)?.Children
                    }).ToList()
                };

                return root;
            }
        }
        private string GetParentImage(string PNo)
        {
            string Base64String = "";
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var product = db.PRODUCTS.Where(p => p.PNo.CompareTo(PNo) == 0).FirstOrDefault();
                var part = db.PARTS.Where(p => p.PNo.CompareTo(PNo) == 0).FirstOrDefault();
                if (product != null)
                {
                    var productImage = db.PRODUCTIMAGES.FirstOrDefault(pi => pi.ProdID == product.ID);
                    if (productImage != null)
                    {
                        Base64String = "data:image/jpeg;base64," + Convert.ToBase64String(productImage.Image);
                    }
                }
                else
                {
                    var partImage = db.PARTIMAGES.FirstOrDefault(pi => pi.PartID == part.ID);
                    if (partImage != null)
                    {
                        Base64String = "data:image/jpeg;base64," + Convert.ToBase64String(partImage.Image);
                    }
                }
                return Base64String;
            }
        }
        private string GetChildImage(string PNo)
        {

            string Base64String = "";
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var part = db.PARTS.Where(p => p.PNo.CompareTo(PNo) == 0).FirstOrDefault();
                var partImage = db.PARTIMAGES.FirstOrDefault(pi => pi.PartID == part.ID);
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

                return View(new BOMCreateProductViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BOMCreateProductViewModel model, BoomViewModel vmodel)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var product = db.PRODUCTS.FirstOrDefault(x => x.ID == model.ProductID && x.IsDeleted == false);
                model.PRODUCT = product;
                model.ProductNo = product.PNo;

                var userID = GetUserID(User.Identity.Name);

                List<BomPart> newList = new List<BomPart>();

                foreach (var part in model.BomList)
                {
                    var _part = db.PARTS.Include("UNIT").Where(p => p.ID == part.PartID).FirstOrDefault();
                    var unitId = db.UNITS.Where(x => x.UnitName == _part.UNIT.ShortName).FirstOrDefault();
                    TEMPORARY_BOMS tempBom = new TEMPORARY_BOMS();
                    tempBom.Consumption = part.Quantity;
                    tempBom.ConsumptionUnitID = unitId.ID;
                    tempBom.ParentPNo = model.ProductNo;
                    tempBom.ChildPNo = _part.PNo;
                    tempBom.IsDeleted = false;
                    tempBom.IsActive = true;
                    tempBom.IsInHouse = part.InHouse;
                    tempBom.UserID = userID.Value;
                    tempBom.IssuedDate = DateTime.Now;
                    db.TEMPORARY_BOMS.Add(tempBom);
                    db.SaveChanges();
                }
                vmodel.ParentPnoComplationStatus = product.PNo;

            }
            return RedirectToAction("CompletionStatus", vmodel);
        }

        public ActionResult CreateWizard(BomViewModel model)
        {
            if (model == null)
                return RedirectToAction("Create");
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var createViewModel = new BOMCreateViewModel();
                var part = db.PARTS.FirstOrDefault(x => x.ID == model.PartID && x.IsDeleted == false);
                createViewModel.Part = part;
                createViewModel.PartID = model.PartID;
                createViewModel.PartNo = model.PartNo;
                createViewModel.SelectedProcessIds = model.SelectedProcessIds;
                createViewModel.Process = model.Process;
                createViewModel.IsActive = model.IsActive;
                createViewModel.ProductPNo = model.ProductPno;



                var part2 = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part2, "ID", "PNo");

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
            if (ModelState.IsValid)
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
                        if (model.PartNo != null) { bom.IsParentProduct = true; }
                        else { bom.IsParentProduct = false; }
                        bom.IsActive = true;
                        bom.WasteAmount = (part_before1.PWeight / part_before1.PWidth * cutterLines1 * cutterWidth1);
                        bom.ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Slitting")?.ID;
                        bom.Consumption = selectedSlittingNorm.WeightOfSlittedParts / cutterLines1;
                        bom.ConsumptionUnit = part_before1.UNIT.ShortName;
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
                                    ConsumptionUnit = part_before.UNIT.ShortName,
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
                                    ChildPNo = part_after_slitting.PNo,
                                    ParentPNo = part_after_Blanking.PNo,
                                    IsDeleted = false,
                                    IsActive = true,
                                    ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Blanking")?.ID,
                                    WasteAmount = Math.Round((part_after_slitting.PWeight / part_after_slitting.PWidth), 2, MidpointRounding.ToEven),
                                    Consumption = Math.Round((int)(part_after_slitting.PWidth * part_after_slitting.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density) / (part_after_slitting.PWeight / part_after_Blanking.PWeight), 2, MidpointRounding.ToEven),
                                    ConsumptionUnit = part_after_slitting.UNIT.ShortName,
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
                                        ConsumptionUnit = part_after_Blanking.UNIT.UnitName,
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
                                            ChildPNo = part_after_Blanking.PNo,
                                            ParentPNo = part_after_stamping.PNo,
                                            IsDeleted = false,
                                            IsActive = true,
                                            ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Blanking")?.ID,
                                            Consumption = (int)(part_after_slitting.PWidth * part_after_slitting.PLength * part_after_Blanking.Gauge * model.BLANKING_NORMS.Density) / (part_after_slitting.PWeight / part_after_Blanking.PWeight),
                                            ConsumptionUnit = part_after_Blanking.UNIT.ShortName,
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
                                            ConsumptionUnit = part_after_Blanking.UNIT.ShortName,
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
                                            ConsumptionUnit = part_after_Blanking.UNIT.ShortName,
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
                                            ConsumptionUnit = part_after_stamping.UNIT.ShortName,
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
                                ParentPNo = model.PartNo,
                                IsDeleted = false,
                                IsActive = true,
                                ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Welding")?.ID,
                                ConsumptionUnit = unit_part.UNIT.ShortName,
                                Consumption = part.WeldingQuantity,
                                Sequence = sequence + 4,
                            };
                            db.BOMS.Add(bom);
                        }
                    }
                    if (model.AssemblyPart != null)
                    {
                        foreach (var part in model.AssemblyPart)
                        {
                            var parts = model.AssemblyPart;
                            var assamble_part = db.PARTS.FirstOrDefault(x => x.ID == part.Assamble_PartID && x.IsDeleted == false);
                            var bom = new BOM();
                            bom.ChildPNo = assamble_part.PNo;
                            bom.ParentPNo = model.PartNo;
                            bom.IsDeleted = false;
                            bom.IsActive = true;
                            bom.ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Assembly")?.ID;
                            bom.ConsumptionUnit = assamble_part.UNIT.ShortName;
                            bom.Sequence = sequence + 5;
                            bom.Consumption = part.AssemblyQuantity;

                            db.BOMS.Add(bom);
                        }
                    }
                    if (model.PaintingPart != null)
                    {
                        foreach (var part in model.PaintingPart)
                        {
                            var parts = model.PaintingPart;
                            var paint_part = db.PARTS.FirstOrDefault(x => x.ID == part.Painting_PartID && x.IsDeleted == false);
                            var bom = new BOM();
                            bom.ChildPNo = paint_part.PNo;
                            bom.ParentPNo = model.PartNo;
                            bom.IsDeleted = false;
                            bom.IsActive = true;
                            bom.ProcessID = processNames.FirstOrDefault(p => p.ProcessName == "Painting")?.ID;
                            bom.ConsumptionUnit = paint_part.UNIT.ShortName;
                            bom.Sequence = sequence + 6;
                            bom.Consumption = part.PaintingQuantity;
                            db.BOMS.Add(bom);
                        }
                    }
                    BoomViewModel vmodel = new BoomViewModel();
                    vmodel.ParentPnoComplationStatus = model.ProductPNo;

                    db.SaveChanges();
                    return RedirectToAction("CompletionStatus", vmodel);
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
        public ActionResult BomCreate(int ID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var process = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false && x.ProcessName != "Assembly" && x.ProcessName != "Painting").ToList();
                var prod = db.TEMPORARY_BOMS.Where(x => x.ID == ID).FirstOrDefault();
                ViewBag.Process = new MultiSelectList(process, "ID", "ProcessName");
                var temp = db.TEMPORARY_BOMS.Where(x => x.ID == ID && x.IsDeleted == false).FirstOrDefault();
                var part = db.PARTS.Where(x => x.PNo == temp.ChildPNo).FirstOrDefault();
                BomViewModel model = new BomViewModel();
                model.Part = part;
                model.ProductPno = prod.ParentPNo;
                return View(model);
            }
        }
        public ActionResult BomCreateDetails(int ID, BOMCreateProductViewModel model1)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var temp = db.TEMPORARY_BOMS.Where(x => x.ID == ID && x.IsDeleted == false).FirstOrDefault();
                var part = db.PARTS.Where(x => x.PNo == temp.ChildPNo).FirstOrDefault();
                BoomViewModel model = new BoomViewModel();
                model.ParentPNo = temp.ParentPNo;
                try
                {
                    if (part != null)
                    {
                        var rootItem = GetBomTree(part.PNo);
                        var boom = new BoomViewModel();
                        boom.ParentPnoComplationStatus = temp.ParentPNo;
                        boom.ParentPNo = rootItem.ParentPNo;
                        boom.ChildPNo = rootItem.ChildPNo;
                        boom.Consumption = rootItem.Consumption;
                        boom.ParentImageBase64 = rootItem.ParentImageBase64;
                        boom.ChildImageBase64 = rootItem.ChildImageBase64;
                        boom.ConsumptionUnit = rootItem.ConsumptionUnit;
                        boom.Children = rootItem.Children;


                        return View(boom);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex);
                }
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult SaveBom(BoomViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var userID = GetUserID(User.Identity.Name);
                var tempBom = db.TEMPORARY_BOMS.FirstOrDefault(tb => tb.ChildPNo == model.ParentPNo && tb.UserID == userID && tb.IsDeleted == false);
                if (tempBom != null)
                {
                    tempBom.NormConfirmed = true;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("CompletionStatus", model);
        }

        [HttpPost]
        public ActionResult BomCreate(BomViewModel model, int[] processID)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var selectedProcesses = db.PRODUCTIONPROCESSES
                                              .Where(x => processID.Contains(x.ID) && x.IsDeleted == false)
                                              .ToList();
                    model.Process = string.Join(", ", selectedProcesses.Select(p => p.ProcessName));
                    model.SelectedProcessIds = processID;
                }
                return RedirectToAction("CreateWizard", model);
            }
            return View(model);
        }
        public ActionResult CompletionStatus(BoomViewModel model)
        {
            var userID = GetUserID(User.Identity.Name);
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var bomlists = db.TEMPORARY_BOMS.Where(x => x.UserID == userID && x.IsDeleted == false && x.ParentPNo == model.ParentPnoComplationStatus).ToList();
                var newLists = new List<BOM>();
                foreach (var bomlist in bomlists)
                {
                    var boms = db.BOMS.Where(x => x.ParentPNo == bomlist.ChildPNo && x.IsDeleted == false).FirstOrDefault();
                    if (boms != null)
                    {
                        newLists.Add(boms);
                    }
                }
                ViewBag.bom = newLists;
                ViewBag.partList = bomlists;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateBom(BoomViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var prod = model.ParentPnoComplationStatus;
                var userID = GetUserID(User.Identity.Name);
                var bomlists = db.TEMPORARY_BOMS.Where(x => x.UserID == userID && x.IsDeleted == false && x.ParentPNo == model.ParentPnoComplationStatus).ToList();
                foreach (var bomlist in bomlists)
                {
                    var oldbom = db.BOMS.Where(x => x.ParentPNo == bomlist.ChildPNo && x.IsDeleted == false).FirstOrDefault();
                    var product = db.PRODUCTS.Where(x => x.PNo == model.ParentPnoComplationStatus).FirstOrDefault();
                    var bom = new BOM();
                    bom.ChildPNo = bomlist.ChildPNo;
                    bom.ParentPNo = model.ParentPnoComplationStatus;
                    bom.Consumption = bomlist.Consumption.Value;
                    bom.ConsumptionUnit = db.UNITS.Where(x => x.ID == bomlist.ConsumptionUnitID).Select(x => x.UnitName).FirstOrDefault();
                    bom.IsParentProduct = model.ParentPnoComplationStatus == product.PNo;
                    bom.IsDeleted = false;
                    bom.IsActive = true;
                    if (oldbom != null)
                    {
                        bom.ProcessID = oldbom.ProcessID;
                    }
                    else
                    {
                        bom.ProcessID = db.PRODUCTIONPROCESSES.Where(x => x.ProcessName == "Welding").Select(x => x.ID).FirstOrDefault();
                    }
                    db.BOMS.Add(bom);
                    db.SaveChanges();


                }
                var tempbom = db.TEMPORARY_BOMS.Where(x => x.UserID == userID && x.IsDeleted == false).ToList();
                db.TEMPORARY_BOMS.RemoveRange(tempbom);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public ActionResult EditBom(int ID, BoomViewModel model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var bomlist = new List<BOM>();
                var processlist = new List<string>();
                var userId = GetUserID(User.Identity.Name);
                var tempbom = db.TEMPORARY_BOMS.Where(x => x.ID == ID && x.IsDeleted == false && x.UserID == userId).FirstOrDefault();
                var bom = db.BOMS.Where(x => x.IsDeleted == false && x.ParentPNo == tempbom.ChildPNo).FirstOrDefault();
                var child_bom = db.BOMS.Where(x => x.IsDeleted == false && x.ParentPNo == bom.ChildPNo).FirstOrDefault();
                var child_bom_child = db.BOMS.Where(x => x.IsDeleted == false && x.ParentPNo == child_bom.ChildPNo).FirstOrDefault();
                bomlist.Add(child_bom_child);
                bomlist.Add(child_bom);
                bomlist.Add(bom);
                var editviewmodel = new BomEditViewModels();
                foreach (var boms in bomlist)
                {
                    var processname = db.PRODUCTIONPROCESSES.Where(x => x.ID == boms.ProcessID && x.IsDeleted == false).FirstOrDefault();
                    var part_befor = db.PARTS.Where(x => x.PNo == boms.ChildPNo && x.IsDeleted == false).FirstOrDefault();
                    var part_after = db.PARTS.Where(x => x.PNo == boms.ParentPNo && x.IsDeleted == false).FirstOrDefault();

                    switch (processname.ProcessName)
                    {
                        case "Slitting":
                            var slitting_norm = db.SLITTING_NORMS.Where(x => x.PartID_after == part_after.ID && x.PartID_before == part_befor.ID && x.IsDeleted == false).FirstOrDefault();
                            editviewmodel.SLITTING_NORMS = slitting_norm;
                            editviewmodel.Slitting_After_ID = part_after.ID;
                            editviewmodel.Slitting_Before_ID = part_befor.ID;
                            break;
                        case "Blanking":
                            var blanking_norm = db.BLANKING_NORMS.Where(x => x.PartID_after == part_after.ID && x.PartID_before == part_befor.ID && x.IsDeleted == false).FirstOrDefault();
                            editviewmodel.BLANKING_NORMS = blanking_norm;
                            editviewmodel.Blanking_After_ID = part_after.ID;
                            editviewmodel.Blanking_Before_ID = part_befor.ID;
                            break;
                        case "Stamping":
                            var stamping_norm = db.STAMPING_NORMS.Where(x => x.PartID_after == part_after.ID && x.PartID_before == part_befor.ID && x.IsDeleted == false).FirstOrDefault();
                            editviewmodel.STAMPING_NORMS = stamping_norm;
                            editviewmodel.Stamping_After_ID = part_after.ID;
                            editviewmodel.Stamping_Before_ID = part_befor.ID;
                            break;
                    }
                    processlist.Add(processname.ProcessName);
                }
                editviewmodel.ProductPNo = model.ParentPnoComplationStatus;
                editviewmodel.PartPno = tempbom.ChildPNo;
                editviewmodel.ProccessList = processlist;
                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");
                return View(editviewmodel);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBom(BomEditViewModels model)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var bomlist = new List<BOM>();
                    var processlist = new List<string>();
                    var userId = GetUserID(User.Identity.Name);
                    var tempbom = db.TEMPORARY_BOMS.Where(x => x.ChildPNo == model.PartPno && x.IsDeleted == false && x.UserID == userId).FirstOrDefault();

                    var bom = db.BOMS.Where(x => x.IsDeleted == false && x.ParentPNo == tempbom.ChildPNo).FirstOrDefault();
                    var child_bom = db.BOMS.Where(x => x.IsDeleted == false && x.ParentPNo == bom.ChildPNo).FirstOrDefault();
                    var child_bom_child = db.BOMS.Where(x => x.IsDeleted == false && x.ParentPNo == child_bom.ChildPNo).FirstOrDefault();
                    bomlist.Add(child_bom_child);
                    bomlist.Add(child_bom);
                    bomlist.Add(bom);
                    var editviewmodel = new BomEditViewModels();
                    foreach (var boms in bomlist)
                    {
                        var processname = db.PRODUCTIONPROCESSES.Where(x => x.ID == boms.ProcessID && x.IsDeleted == false).FirstOrDefault();

                            switch (processname.ProcessName)
                            {
                                case "Slitting":
                                    var slitting_norm = db.SLITTING_NORMS.Where(x => x.PartID_after == model.Slitting_After_ID && x.PartID_before == model.Slitting_Before_ID && x.IsDeleted == false).FirstOrDefault();
                                    if (slitting_norm != null)
                                    {
                                        slitting_norm.CutterWidth = model.SLITTING_NORMS.CutterWidth;
                                        db.Entry(slitting_norm).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    break;
                                case "Blanking":
                                    var blanking_norm = db.BLANKING_NORMS.Where(x => x.PartID_after == model.Blanking_After_ID && x.PartID_before == model.Blanking_Before_ID && x.IsDeleted == false).FirstOrDefault();
                                    if (blanking_norm != null)
                                    {
                                        blanking_norm.Density = model.BLANKING_NORMS.Density;
                                        db.Entry(blanking_norm).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    break;
                                case "Stamping":
                                    var stamping_norm = db.STAMPING_NORMS.Where(x => x.PartID_after == model.Stamping_After_ID && x.PartID_before == model.Stamping_Before_ID && x.IsDeleted == false).FirstOrDefault();
                                    if (stamping_norm != null)
                                    {
                                        db.Entry(stamping_norm).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    break;
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