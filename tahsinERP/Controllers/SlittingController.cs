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
            return View();
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
        [ValidateAntiForgeryToken]
        public ActionResult Create(SLITTING_NORMS slitting_norms)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var part_before = db.PARTS.Where(x => x.IsDeleted == false && x.ID == slitting_norms.PartID_before).FirstOrDefault();
                        var part_after = db.PARTS.Where(x => x.IsDeleted == false && x.ID == slitting_norms.PartID_after).FirstOrDefault();
                        //var userid = db.USERS.FirstOrDefault(x => x.Uname == User.Identity.Name);

                        // Ensure parts and user are not null
                        if (part_before == null || part_after == null )
                        {
                            ModelState.AddModelError(string.Empty, "One of the required entities was not found.");
                            var part = db.PARTS.Where(pr => pr.IsDeleted == false).ToList();
                            ViewBag.Part = new SelectList(part, "ID", "PNo");
                            return View(slitting_norms);
                        }
                        var cutterLines = (int)((part_before.PWidth) / (part_after.PWidth) - 1);
                        var cutterWidth = slitting_norms.CutterWidth;
                        var pieceCount = Convert.ToInt32(Math.Round(part_before.PWidth / part_after.PWidth));
                        
                        var slitting_process = new SLITTING_NORMS
                        {
                            PartID_after = slitting_norms.PartID_after,
                            PartID_before = slitting_norms.PartID_before,
                            SlittingPieces = pieceCount,
                            CutterLines = cutterLines,
                            CutterWidth = cutterWidth,
                            WeightOfSlittedParts = part_after.PWidth * (part_before.PWeight / part_before.PWidth),
                            WeightOfCutWaste = (part_before.PWeight / part_before.PWidth * cutterLines * cutterWidth),
                            WidthOfUsefulWaste = part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines - cutterWidth),
                            WeightOfUsefulWaste = (part_before.PWidth - (pieceCount * part_after.PWidth) - (cutterLines - cutterWidth)) * (part_before.PWeight / part_before.PWidth),
                            IssuedDateTime = DateTime.Now
                        };

                        db.SLITTING_NORMS.Add(slitting_process);
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
            return View(slitting_norms);
        }


    }
}