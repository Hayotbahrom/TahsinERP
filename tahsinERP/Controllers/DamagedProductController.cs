using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class DamagedProductController : Controller
    {
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var damagedProducts = db.DAMAGED_PRODUCTS
                                        .Where(dp => dp.IsDeleted == false)
                                        .Include(dp => dp.DEFECT_TYPES)
                                        .Include(dp => dp.PRODUCT)
                                        .ToList();
                return View(damagedProducts);
            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Product = new SelectList(products, "ID", "PNo");
                var defect_types = db.DEFECT_TYPES.Where(x => x.IsDeleted == false).ToList();
                ViewBag.DefectType = new SelectList(defect_types, "ID", "DefectType");
                return View();
            }
        }
        [HttpPost]
        public ActionResult Create(DAMAGED_PRODUCTS model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        model.IsDeleted = false;
                        model.IssueDateTime = DateTime.Now;
                        db.DAMAGED_PRODUCTS.Add(model);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
                return View(model);
            }
        }
        public ActionResult Edit(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var damaged_product = db.DAMAGED_PRODUCTS.Find(id);

                if (damaged_product == null)
                {
                    return HttpNotFound();
                }
                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Product = new SelectList(products, "ID", "PNo");
                var defect_types = db.DEFECT_TYPES.Where(x => x.IsDeleted == false).ToList();
                ViewBag.DefectType = new SelectList(defect_types, "ID", "DefectType");

                return View(damaged_product);
            }
        }
        [HttpPost]
        public ActionResult Edit(DAMAGED_PRODUCTS damagedProduct)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var damaged_product = db.DAMAGED_PRODUCTS.Find(damagedProduct.ID);
                    if (damaged_product == null)
                    {
                        return HttpNotFound();
                    }
                    damaged_product.ProductID = damagedProduct.ProductID;
                    damaged_product.DefectTypeID = damagedProduct.DefectTypeID;
                    damaged_product.Quantity = damagedProduct.Quantity;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                var products = db.PRODUCTS.Where(x => x.IsDeleted == false).ToList();
                ViewBag.Product = new SelectList(products, "ID", "PNo", damagedProduct.ProductID);
                var defect_types = db.DEFECT_TYPES.Where(x => x.IsDeleted == false).ToList();
                ViewBag.DefectType = new SelectList(defect_types, "ID", "DefectType", damagedProduct.DefectTypeID);

                return View(damagedProduct);
            }
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var damagedProduct = db.DAMAGED_PRODUCTS
                                       .Include(dp => dp.DEFECT_TYPES)
                                       .Include(dp => dp.PRODUCT)
                                       .FirstOrDefault(dp => dp.ID == id);

                if (damagedProduct == null)
                {
                    return HttpNotFound();
                }

                return View(damagedProduct);
            }
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var damagedProduct = db.DAMAGED_PRODUCTS.Find(id);

                if (damagedProduct == null)
                {
                    return HttpNotFound();
                }

                damagedProduct.IsDeleted = true;
                db.Entry(damagedProduct).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var damagedProduct = db.DAMAGED_PRODUCTS
                                       .Include(dp => dp.DEFECT_TYPES)
                                       .Include(dp => dp.PRODUCT)
                                       .FirstOrDefault(dp => dp.ID == id);

                if (damagedProduct == null)
                {
                    return HttpNotFound();
                }

                return View(damagedProduct);
            }
        }
    }
}
