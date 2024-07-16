using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class SlittingController : Controller
    {
        // GET: Slitting
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var slitting = db.SLITTING_NORMS
                    .Where(x => x.IsDeleted == false)
                    .Select(s => new
                    {
                        SlittingNorm = s,
                        PartAfter = db.PARTS.FirstOrDefault(p => p.ID == s.PartID_after && p.IsDeleted == false),
                        PartBefore = db.PARTS.FirstOrDefault(p => p.ID == s.PartID_before && p.IsDeleted == false)
                    })
                    .ToList();

                var viewModel = slitting.Select(s => new SlittingNormViewModel
                {
                    SLITTING_NORMS = s.SlittingNorm,
                    PartAfter = s.PartAfter,
                    PartBefore = s.PartBefore,
                }).ToList();

                return View(viewModel);
            }
        }


        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");
                return View();
            }
        }

        [HttpPost]
        public ActionResult Create(SLITTING_NORMS model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var part_before = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_before);
                        var part_after = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_after);

                        var cutterWidth = model.CutterWidth;
                        var pieceCount = Convert.ToInt32(Math.Floor(part_before.PWidth / part_after.PWidth));
                        var cutterLines = (pieceCount - 1);

                        var userImageCookie = Request.Cookies["UserImageId"];
                        int userImageId = int.Parse(userImageCookie.Value);

                        var userimage = db.USERIMAGES.FirstOrDefault(x => x.ID == userImageId);
                        int userId = userimage.UserID;

                        if (part_after != null && part_before != null)
                        {
                            var slitting_process = new SLITTING_NORMS
                            {
                                IsDeleted = false,
                                IsActive = model.IsActive,
                                PartID_after = model.PartID_after,
                                PartID_before = model.PartID_before,
                                SlittingPieces = pieceCount,
                                CutterLines = cutterLines,
                                CutterWidth = cutterWidth,
                                WeightOfSlittedParts = Math.Round((part_after.PWidth * (part_before.PWeight / part_before.PWidth)), 2 , MidpointRounding.ToEven),
                                WeightOfCutWaste = Math.Round(((part_before.PWeight / part_before.PWidth) * cutterLines * cutterWidth), 2 ,MidpointRounding.ToEven),
                                WidthOfUsefulWaste = Math.Round((part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines * cutterWidth)),2, MidpointRounding.ToEven),
                                WeightOfUsefulWaste = Math.Round(((part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines * cutterWidth)) * (part_before.PWeight / part_before.PWidth)), 2, MidpointRounding.ToEven),
                                IssuedDateTime = DateTime.Now,
                                IssuedByUserID = userId
                            };
                            db.SLITTING_NORMS.Add(slitting_process);
                        }

                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                var partList = db.PARTS.Where(pr => pr.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(partList, "ID", "PNo");
            }
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var slittingNorm = db.SLITTING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (slittingNorm == null)
                {
                    return HttpNotFound();
                }

                var part = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(part, "ID", "PNo");

                return View(slittingNorm);
            }
        }


        [HttpPost]
        public ActionResult Edit(SLITTING_NORMS model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var slittingNorm = db.SLITTING_NORMS.FirstOrDefault(x => x.ID == model.ID && x.IsDeleted == false);
                        if (slittingNorm == null)
                        {
                            return HttpNotFound();
                        }

                        var part_before = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_before);
                        var part_after = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_after);

                        var cutterLines = (int)((part_before.PWidth) / (part_after.PWidth) - 1);
                        var cutterWidth = model.CutterWidth;
                        var pieceCount = Convert.ToInt32(Math.Round(part_before.PWidth / part_after.PWidth));

                        if (part_after != null && part_before != null)
                        {
                            slittingNorm.IsActive = model.IsActive;
                            slittingNorm.PartID_after = model.PartID_after;
                            slittingNorm.PartID_before = model.PartID_before;
                            slittingNorm.SlittingPieces = pieceCount;
                            slittingNorm.CutterLines = cutterLines;
                            slittingNorm.CutterWidth = cutterWidth;
                            slittingNorm.WeightOfSlittedParts = part_after.PWidth * (part_before.PWeight / part_before.PWidth);
                            slittingNorm.WeightOfCutWaste = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth);
                            slittingNorm.WidthOfUsefulWaste = part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines - cutterWidth);
                            slittingNorm.WeightOfUsefulWaste = (part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines - cutterWidth)) * (part_before.PWeight / part_before.PWidth);
                            slittingNorm.IssuedDateTime = DateTime.Now; // This might not be necessary for an edit
                            slittingNorm.IssuedByUserID = 5; // This might not be necessary for an edit
                        }

                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                var partList = db.PARTS.Where(pr => pr.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(partList, "ID", "PNo");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var slittingNorm = db.SLITTING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (slittingNorm == null)
                {
                    return HttpNotFound();
                }

                var partBefore = db.PARTS.FirstOrDefault(p => p.ID == slittingNorm.PartID_before && p.IsDeleted == false);
                var partAfter = db.PARTS.FirstOrDefault(p => p.ID == slittingNorm.PartID_after && p.IsDeleted == false);

                var viewModel = new SlittingNormViewModel
                {
                    SLITTING_NORMS = slittingNorm,
                    PartBefore = partBefore,
                    PartAfter = partAfter
                };

                return View(viewModel);
            }
        }


        [HttpPost]
        public ActionResult Delete(int id,FormCollection fcn)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var slittingNorm = db.SLITTING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (slittingNorm == null)
                {
                    return HttpNotFound();
                }

                slittingNorm.IsDeleted = true;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }


    }
}