using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PartWRHSController : Controller
    {
        // GET: PartWRHS
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var part_wrhs = db.PART_WRHS
                          .Where(x => x.IsDeleted == false)
                          .Include(x => x.USER)
                          .ToList();
                return View(part_wrhs);
            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // ViewBag orqali "Shop" ma'lumotlari
                var shops = db.PROD_SHOPS.ToList();
                ViewBag.ShopList = new SelectList(shops, "ID", "ShopName");

                // "MRP" ma'lumotlarini olish uchun
                var mrpUsers = db.USERS
                                    .Where(u => u.ROLES.Any(r => r.RName == "MRP"))
                                    .ToList();
                ViewBag.MRPUsers = new SelectList(mrpUsers, "ID", "FullName");

                return View();
            }
        }

        // POST: YourController/Create
        [HttpPost]
        public ActionResult Create(PART_WRHS model, int MRPUserID, int ShopID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    // Set MRP and ShopID properties
                    model.MRP = MRPUserID;
                    model.ShopID = ShopID;
                    model.IsDeleted = false;

                    // PART_WRHS-ni saqlash
                    db.PART_WRHS.Add(model);
                    db.SaveChanges();

                    // PART_WRHS bilan bog'liq MRP foydalanuvchisini yangilash
                    var mrpUser = db.USERS.Find(MRPUserID);
                    if (mrpUser != null)
                    {
                        mrpUser.PART_WRHS.Add(model);
                        db.Entry(mrpUser).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    return RedirectToAction("Index"); // Muaffaqiyatli saqlandi, index sahifasiga yo'naltirish
                }

                // Agar ModelState noto'g'ri bo'lsa, qaytadan "Create" sahifasini ko'rsatish
                var shops = db.PROD_SHOPS.ToList();
                ViewBag.ShopList = new SelectList(shops, "ID", "ShopName", model.ShopID);

                var mrpUsers = db.USERS
                                    .Where(u => u.ROLES.Any(r => r.RName == "MRP"))
                                    .ToList();
                ViewBag.MRPUsers = new SelectList(mrpUsers, "ID", "FullName", MRPUserID);

                return View(model);
            }
        }
        private readonly DBTHSNEntities db = new DBTHSNEntities();

        // GET: PartWRHS/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                PART_WRHS partWRHS = db.PART_WRHS.Include(p => p.USER).FirstOrDefault(p => p.ID == id);
                if (partWRHS == null)
                {
                    return HttpNotFound();
                }

                ViewBag.MRPUsers = new SelectList(db.USERS.Where(u => u.ROLES.Any(r => r.RName == "MRP")), "ID", "FullName", partWRHS.MRP);
                ViewBag.ShopList = new SelectList(db.PROD_SHOPS, "ID", "ShopName", partWRHS.ShopID);
                return View(partWRHS);
            }
            catch (Exception ex)
            {
                // Log the error (uncomment the following line to write to a log file)
                // Log.Error(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }

        // POST: PartWRHS/Edit/5
        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,WHName,Description,IsDeleted,MRP,ShopID")] PART_WRHS partWRHS)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    partWRHS.IsDeleted = false;
                    db.Entry(partWRHS).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the error (uncomment the following line to write to a log file)
                    // Log.Error(ex);
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            ViewBag.MRPUsers = new SelectList(db.USERS.Where(u => u.ROLES.Any(r => r.RName == "MRP")), "ID", "FullName", partWRHS.MRP);
            ViewBag.ShopList = new SelectList(db.PROD_SHOPS, "ID", "ShopName", partWRHS.ShopID);
            return View(partWRHS);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PART_WRHS partWRHS = db.PART_WRHS.Find(id);
            if (partWRHS == null)
            {
                return HttpNotFound();
            }

            return View(partWRHS);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                PART_WRHS partWRHS = db.PART_WRHS.Find(id);
                if (partWRHS != null)
                {
                    partWRHS.IsDeleted = true;
                    db.Entry(partWRHS).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                PART_WRHS partWRHS = db.PART_WRHS.Include(p => p.USER).Include(p => p.PROD_SHOPS).FirstOrDefault(p => p.ID == id);
                if (partWRHS == null)
                {
                    return HttpNotFound();
                }

                return View(partWRHS);
            }
            catch (Exception ex)
            {
                // Log the error (uncomment the following line to write to a log file)
                // Log.Error(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}