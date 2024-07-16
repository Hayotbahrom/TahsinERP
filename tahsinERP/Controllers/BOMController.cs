using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
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
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                int sequence = 0;
                var processNames = db.PRODUCTIONPROCESSES.Where(x => x.IsDeleted == false).ToList();
                var userId = GetUserID(User.Identity.Name);

                if(model.SLITTING_NORMS != null)
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
                            IssuedByUserID = userId.GetValueOrDefault(),
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


            }

            return View();

        }
        

            

    }
}