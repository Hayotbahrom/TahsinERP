using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ProductPlanController : Controller
    {
        // GET: ProductPlan
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                var productplan = db.PRODUCTPLANS.Where(x => x.IsDeleted == false).ToList();
                return View(productplan);
            }
        }

        public ActionResult Details(int? id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                PRODUCTPLAN productPlan = db.PRODUCTPLANS
                                           .Include(pp => pp.PRODUCT)
                                           .FirstOrDefault(pp => pp.ID == id);

                if (productPlan == null)
                {
                    return HttpNotFound();
                }

                return View(productPlan);

            }
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "Name");
                return View();
            }
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "ProductID, PlannedQty, Label, IssueDate, DueDate, IsDeleted")] PRODUCTPLAN productPlan)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        productPlan.IsDeleted = false;
                        db.PRODUCTPLANS.Add(productPlan);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
                ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "Name", productPlan.ProductID);
            }
            return View(productPlan);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PRODUCTPLAN productPlan = db.PRODUCTPLANS.Find(id);
                if (productPlan == null)
                {
                    return HttpNotFound();
                }

                ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "Name", productPlan.ProductID);
                return View(productPlan);
            }
        }
        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,ProductID,PlannedQty,Label,IssueDate,DueDate,IsDeleted")] PRODUCTPLAN productPlan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        var existingProductPlan = db.PRODUCTPLANS.Find(productPlan.ID);
                        if (existingProductPlan != null)
                        {
                            existingProductPlan.ProductID = productPlan.ProductID;
                            existingProductPlan.Amount = productPlan.Amount;
                            existingProductPlan.StartDate = productPlan.StartDate;
                            existingProductPlan.DueDate = productPlan.DueDate;
                            existingProductPlan.IsDeleted = false;

                            db.Entry(existingProductPlan).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Tasdiqlanmadi: " + ex.Message);
                }
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "Name", productPlan.ProductID);
            }

            return View(productPlan);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PRODUCTPLAN productPlan = db.PRODUCTPLANS.Find(id);
                if (productPlan == null)
                {
                    return HttpNotFound();
                }

                ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "Name", productPlan.ProductID);
                return View(productPlan);
            }
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                try
                {
                    PRODUCTPLAN productPlan = db.PRODUCTPLANS.Find(id);
                    if (productPlan == null)
                    {
                        return HttpNotFound();
                    }

                    productPlan.IsDeleted = true;
                    db.Entry(productPlan).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    ModelState.AddModelError("", ex.Message);
                    return View();
                }
            }
        }
    }
}