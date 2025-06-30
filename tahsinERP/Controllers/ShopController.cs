using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var prod_shop = db.SHOPS.Where(x => x.IsDeleted == false).ToList();

                return View(prod_shop);
            }
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "ShopName,CompanyID,Description,Isdeleted")] SHOP prod_shop)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        prod_shop.IsDeleted = false;
                        prod_shop.CompanyID = 1;
                        db.SHOPS.Add(prod_shop);
                        db.SaveChanges();

                        LogHelper.LogToDatabase(User.Identity.Name, "ShopController", $"{prod_shop.ShopName} - Shopni yaratdi");

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                }
                return View(prod_shop);
            }
        }

        public ActionResult Delete(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var prod_shop = db.SHOPS.Find(id);
                if (prod_shop == null)
                {
                    return HttpNotFound();
                }
                return View(prod_shop);
            }
        }
        [HttpPost]
        public ActionResult Delete(SHOP prod_shop)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var prod_shop_deleted = db.SHOPS.Find(prod_shop.ID);
                    if (prod_shop_deleted != null)
                    {
                        prod_shop_deleted.IsDeleted = true;
                        if (TryUpdateModel(prod_shop_deleted, "", new string[] { "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "ShopController", $"{prod_shop_deleted.ShopName} - Shopni o'chirdi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(prod_shop_deleted);
                }
            }

            return View();
        }

        public ActionResult Edit(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var prod_shop = db.SHOPS.Find(id);
                if (prod_shop == null)
                {
                    return HttpNotFound();
                }
                return View(prod_shop);
            }
        }
        [HttpPost]
        public ActionResult Edit(SHOP prod_shop)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var prod_shop_Update = db.SHOPS.Find(prod_shop.ID);
                    if (prod_shop_Update != null)
                    {
                        prod_shop_Update.IsDeleted = false;
                        if (TryUpdateModel(prod_shop_Update, "", new string[] { "ShopName", "Description", "CompanyID", "IsDeleted" }))
                        {
                            try
                            {
                                db.SaveChanges();

                                LogHelper.LogToDatabase(User.Identity.Name, "ShopController", $"{prod_shop_Update.ShopName} - Shopni tahrirladi");

                                return RedirectToAction("Index");
                            }
                            catch (RetryLimitExceededException)
                            {
                                ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                            }
                        }
                    }
                    return View(prod_shop_Update);
                }
            }
            return View();
        }
        public ActionResult Details(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var prod_shop = db.SHOPS.Find(id);
                if (prod_shop == null)
                {
                    return HttpNotFound();
                }
                return View(prod_shop); 
            }
        }
    }
}