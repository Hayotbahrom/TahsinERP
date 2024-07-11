using System;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;
using tahsinERP.ViewModels.BOM;

namespace tahsinERP.Controllers
{
    public class BlankingController : Controller
    {
        // GET: Blanking
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var blanking = db.BLANKING_NORMS
                    .Where(x => x.IsDeleted == false)
                    .Select(s => new
                    {
                        Blankingnorms = s,
                        PartAfter = db.PARTS.FirstOrDefault(p => p.ID == s.PartID_after && p.IsDeleted == false),
                        PartBefore = db.PARTS.FirstOrDefault(p => p.ID == s.PartID_before && p.IsDeleted == false)
                    })
                    .ToList();

                var viewModel = blanking.Select(s => new BlankingNormViewModel
                {
                    BLANKING_NORMS = s.Blankingnorms,
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
        public ActionResult Create(BLANKING_NORMS model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var userImageCookie = Request.Cookies["UserImageId"];
                int userImageId = int.Parse(userImageCookie.Value);

                var userimage = db.USERIMAGES.FirstOrDefault(x => x.ID == userImageId);
                int userId = userimage.UserID;
                try
                {
                    if (ModelState.IsValid)
                    {
                        var part_before_Blanking = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_before);
                        var part_after_Blanking = db.PARTS.FirstOrDefault(p => p.IsDeleted == false && p.ID == model.PartID_after);
                        var blanking_norms = new BLANKING_NORMS
                        {
                            IsDeleted = false,
                            IsActive = model.IsActive,
                            PartID_before = part_before_Blanking.ID,
                            PartID_after = part_after_Blanking.ID,
                            Density = model.Density,
                            QuantityOfBlanks = (int)(part_before_Blanking.PWeight / part_after_Blanking.PWeight),
                            WeightOfBlanks = (int)(part_before_Blanking.PWidth * part_before_Blanking.PLength * part_after_Blanking.Gauge * model.Density),
                            WeightOfCutWaste = part_before_Blanking.PWeight - (part_before_Blanking.PWeight * part_after_Blanking.PWeight),
                            IssuedDateTime = DateTime.Now,
                            IssuedByUserID = userId
                        };

                        db.BLANKING_NORMS.Add(blanking_norms);


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
                var blankingNorm = db.BLANKING_NORMS.FirstOrDefault(b => b.ID == id && b.IsDeleted == false);
                if (blankingNorm == null)
                {
                    return HttpNotFound();
                }

                var partList = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(partList, "ID", "PNo");

                return View(blankingNorm);
            }
        }

        [HttpPost]
        public ActionResult Edit(BLANKING_NORMS model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var userImageCookie = Request.Cookies["UserImageId"];
                int userImageId = int.Parse(userImageCookie.Value);

                var userimage = db.USERIMAGES.FirstOrDefault(x => x.ID == userImageId);
                int userId = userimage.UserID;

                try
                {
                    if (ModelState.IsValid)
                    {
                        var blankingNorm = db.BLANKING_NORMS.FirstOrDefault(b => b.ID == model.ID && b.IsDeleted == false);
                        if (blankingNorm == null)
                        {
                            return HttpNotFound();
                        }

                        var part_before_Blanking = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_before);
                        var part_after_Blanking = db.PARTS.FirstOrDefault(p => p.IsDeleted == false && p.ID == model.PartID_after);

                        blankingNorm.IsActive = model.IsActive;
                        blankingNorm.PartID_before = part_before_Blanking.ID;
                        blankingNorm.PartID_after = part_after_Blanking.ID;
                        blankingNorm.Density = model.Density;
                        blankingNorm.QuantityOfBlanks = (int)(part_before_Blanking.PWeight / part_after_Blanking.PWeight);
                        blankingNorm.WeightOfBlanks = (int)(part_before_Blanking.PWidth * part_before_Blanking.PLength * part_after_Blanking.Gauge * model.Density);
                        blankingNorm.WeightOfCutWaste = part_before_Blanking.PWeight - (part_before_Blanking.PWeight * part_after_Blanking.PWeight);
                        blankingNorm.IssuedDateTime = DateTime.Now;
                        blankingNorm.IssuedByUserID = userId;

                        db.Entry(blankingNorm).State = System.Data.Entity.EntityState.Modified;
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
                var blankingNorm = db.BLANKING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (blankingNorm == null)
                {
                    return HttpNotFound();
                }

                var partBefore = db.PARTS.FirstOrDefault(p => p.ID == blankingNorm.PartID_before && p.IsDeleted == false);
                var partAfter = db.PARTS.FirstOrDefault(p => p.ID == blankingNorm.PartID_after && p.IsDeleted == false);

                var viewModel = new BlankingNormViewModel
                {
                    BLANKING_NORMS = blankingNorm,
                    PartBefore = partBefore,
                    PartAfter = partAfter
                };

                return View(viewModel);
            }
        }


        [HttpPost]
        public ActionResult Delete(int id, FormCollection fcn)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var blankingNorm = db.BLANKING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (blankingNorm == null)
                {
                    return HttpNotFound();
                }

                blankingNorm.IsDeleted = true;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        public ActionResult Details(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var blankingNorm = db.BLANKING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (blankingNorm == null)
                {
                    return HttpNotFound();
                }
                var partBefore = db.PARTS.FirstOrDefault(p => p.ID == blankingNorm.PartID_before && p.IsDeleted == false);
                var partAfter = db.PARTS.FirstOrDefault(p => p.ID == blankingNorm.PartID_after && p.IsDeleted == false);

                var viewModel = new BlankingNormViewModel
                {
                    BLANKING_NORMS = blankingNorm,
                    PartBefore = partBefore,
                    PartAfter = partAfter
                };
                return View(viewModel);
            }
        }
    }
}