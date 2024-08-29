using System;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels.BOM;

namespace tahsinERP.Controllers
{
    public class StampingController : Controller
    {
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var stamping = db.STAMPING_NORMS
                    .Where(x => x.IsDeleted == false)
                    .Select(s => new
                    {
                        StampingNorms = s,
                        PartAfter = db.PARTS.FirstOrDefault(p => p.ID == s.PartID_after && p.IsDeleted == false),
                        PartBefore = db.PARTS.FirstOrDefault(p => p.ID == s.PartID_before && p.IsDeleted == false)
                    })
                    .ToList();

                var viewModel = stamping.Select(s => new StampingNormViewModel
                {
                    STAMPING_NORMS = s.StampingNorms,
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
        public ActionResult Create(STAMPING_NORMS model)
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
                        var part_before_Stamping = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_before);
                        var part_after_Stamping = db.PARTS.FirstOrDefault(p => p.IsDeleted == false && p.ID == model.PartID_after);

                        var stamping_norms = new STAMPING_NORMS
                        {
                            IsDeleted = false,
                            IsActive = model.IsActive,
                            PartID_before = part_before_Stamping.ID,
                            PartID_after = part_after_Stamping.ID,
                            Density = model.Density,
                            QuantityOfStamps = (int)(part_before_Stamping.PWeight / part_after_Stamping.PWeight),
                            WeightOfStamps = (int)(part_before_Stamping.PWidth * part_before_Stamping.PLength * part_after_Stamping.Gauge * model.Density),
                            WeightOfWaste = part_before_Stamping.PWeight - (part_before_Stamping.PWeight * part_after_Stamping.PWeight),
                            IssuedDateTime = DateTime.Now,
                            IssuedByUserID = userId
                        };

                        db.STAMPING_NORMS.Add(stamping_norms);
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "StampingController", $"{stamping_norms.ID} ID ga ega StampingNormni yaratdi");

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
                var stampingNorm = db.STAMPING_NORMS.FirstOrDefault(b => b.ID == id && b.IsDeleted == false);
                if (stampingNorm == null)
                {
                    return HttpNotFound();
                }

                var partList = db.PARTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Part = new SelectList(partList, "ID", "PNo");

                return View(stampingNorm);
            }
        }

        [HttpPost]
        public ActionResult Edit(STAMPING_NORMS model)
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
                        var stampingNorm = db.STAMPING_NORMS.FirstOrDefault(b => b.ID == model.ID && b.IsDeleted == false);
                        if (stampingNorm == null)
                        {
                            return HttpNotFound();
                        }

                        var part_before_Stamping = db.PARTS.FirstOrDefault(x => x.IsDeleted == false && x.ID == model.PartID_before);
                        var part_after_Stamping = db.PARTS.FirstOrDefault(p => p.IsDeleted == false && p.ID == model.PartID_after);

                        stampingNorm.IsActive = model.IsActive;
                        stampingNorm.PartID_before = part_before_Stamping.ID;
                        stampingNorm.PartID_after = part_after_Stamping.ID;
                        stampingNorm.Density = model.Density;
                        stampingNorm.QuantityOfStamps = (int)(part_before_Stamping.PWeight / part_after_Stamping.PWeight);
                        stampingNorm.WeightOfStamps = (int)(part_before_Stamping.PWidth * part_before_Stamping.PLength * part_after_Stamping.Gauge * model.Density);
                        stampingNorm.WeightOfWaste = part_before_Stamping.PWeight - (part_before_Stamping.PWeight * part_after_Stamping.PWeight);
                        stampingNorm.IssuedDateTime = DateTime.Now;
                        stampingNorm.IssuedByUserID = userId;

                        db.Entry(stampingNorm).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "StampingController", $"{stampingNorm.ID} ID ga ega StampingNormni tahrirladi");

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
                var stampingNorm = db.STAMPING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (stampingNorm == null)
                {
                    return HttpNotFound();
                }

                var partBefore = db.PARTS.FirstOrDefault(p => p.ID == stampingNorm.PartID_before && p.IsDeleted == false);
                var partAfter = db.PARTS.FirstOrDefault(p => p.ID == stampingNorm.PartID_after && p.IsDeleted == false);

                var viewModel = new StampingNormViewModel
                {
                    STAMPING_NORMS = stampingNorm,
                    PartBefore = partBefore,
                    PartAfter = partAfter
                };

                return View(viewModel);
            }
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(int id, FormCollection fcn)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var stampingNorm = db.STAMPING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (stampingNorm == null)
                {
                    return HttpNotFound();
                }

                stampingNorm.IsDeleted = true;
                db.SaveChanges();

                LogHelper.LogToDatabase(User.Identity.Name, "StampingController", $"{stampingNorm.ID} ID ga ega StampingNormni o'chirdi");

                return RedirectToAction("Index");
            }
        }

        public ActionResult Details(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var stampingNorm = db.STAMPING_NORMS.FirstOrDefault(x => x.ID == id && x.IsDeleted == false);
                if (stampingNorm == null)
                {
                    return HttpNotFound();
                }

                var partBefore = db.PARTS.FirstOrDefault(p => p.ID == stampingNorm.PartID_before && p.IsDeleted == false);
                var partAfter = db.PARTS.FirstOrDefault(p => p.ID == stampingNorm.PartID_after && p.IsDeleted == false);

                var viewModel = new StampingNormViewModel
                {
                    STAMPING_NORMS = stampingNorm,
                    PartBefore = partBefore,
                    PartAfter = partAfter
                };

                return View(viewModel);
            }
        }
    }
}
