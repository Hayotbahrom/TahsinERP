using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class ProductPlanController : Controller
    {
        // GET: ProductPlan
        public ActionResult Index(int? shopID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var shops = db.SHOPS.Where(s => s.IsDeleted == false).ToList();
                if (shopID.HasValue)
                {
                    ViewBag.shopIsNotSelected = false;
                    ViewBag.productList = db.Database.SqlQuery<GetProductListPerShop_Result>("EXEC GetProductListPerShop @ShopID", new SqlParameter("@ShopID", shopID)).ToList();
                    ViewBag.ShopList = new SelectList(shops, "ID", "ShopName", shopID);
                }
                else
                {
                    ViewBag.shopIsNotSelected = true;
                    ViewBag.productList = db.PRODUCTPLANS.Include(pp => pp.PRODUCT).Where(x => x.IsDeleted == false).ToList();
                    ViewBag.ShopList = new SelectList(shops, "ID", "ShopName");
                }
                return View();
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

                ViewBag.dailyPlan = db.PRODUCTPLANS_DAILY.Where(p => p.PlanID == productPlan.ID).ToList();

                return View(productPlan);

            }
        }
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "PNo");
                return View();
            }
        }
        [HttpPost]
        public ActionResult Create(ProductPlanVM productPlan)
        {
            int dayCount = 0;
            double dayPlanAmount;
            DateTime startDate;
            if (!CheckForPlanExistencePerProduct(productPlan))
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            PRODUCTPLAN plan = new PRODUCTPLAN();
                            plan.ProductID = productPlan.ProductID;
                            plan.Amount = productPlan.Amount;
                            plan.StartDate = productPlan.StartDate;
                            plan.DueDate = productPlan.DueDate;
                            plan.IsDeleted = false;

                            db.PRODUCTPLANS.Add(plan);
                            db.SaveChanges();

                            LogHelper.LogToDatabase(User.Identity.Name, "ProductPlanController", $"{productPlan.ID} ID ga ega ProductPlanni yaratdi");

                            dayCount = productPlan.DueDate.Subtract(productPlan.StartDate).Days;
                            dayPlanAmount = Math.Ceiling(plan.Amount / dayCount);
                            startDate = productPlan.StartDate;
                            for (int i = 0; i < dayCount; i++)
                            {
                                PRODUCTPLANS_DAILY dailyPlan = new PRODUCTPLANS_DAILY();
                                if (!productPlan.IsTwoShiftPlan)
                                    dailyPlan.DayShift = dayPlanAmount;
                                else
                                {
                                    dailyPlan.DayShift = Math.Ceiling(dayPlanAmount / 2);
                                    dailyPlan.NightShift = dayPlanAmount - dailyPlan.DayShift;
                                }
                                dailyPlan.PlanID = plan.ID;
                                dailyPlan.Day = startDate;
                                startDate = startDate.AddDays(1);
                                db.PRODUCTPLANS_DAILY.Add(dailyPlan);

                                LogHelper.LogToDatabase(User.Identity.Name, "ProductPlanController", $"{dailyPlan.ID} ID ga ega ProductPlanDailyni yaratdi");
                            }

                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error: " + ex.Message);
                    }
                    ViewBag.ProductList = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "Name", productPlan.ProductID);

                    return View(productPlan);
                }
            }
            else
            {
                ModelState.AddModelError("", "Ushbu maxsulotga kiritlgan muddatlarda reja berilgan, iltimos tekshirib qaytadan urinib ko'ring!");
                return RedirectToAction("Create", productPlan);
            }
        }
        private bool CheckForPlanExistencePerProduct(ProductPlanVM productPlan)
        {
            DateTime startDate = productPlan.StartDate.Date;
            DateTime endDate = productPlan.DueDate.Date;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                PRODUCTPLAN plan = db.PRODUCTPLANS.Where(p => ((p.StartDate <= productPlan.StartDate) || (p.StartDate <= productPlan.DueDate))
                                                                                                     && p.ProductID == productPlan.ProductID
                                                                                                     && p.IsDeleted == false).FirstOrDefault();
                if (plan != null)
                    return true;
                else
                    return false;
            }
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

                            LogHelper.LogToDatabase(User.Identity.Name, "ProductPlanController", $"{productPlan.ID} ID ga ega ProductPlanni tahrirladi");
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

                    LogHelper.LogToDatabase(User.Identity.Name, "ProductPlanController", $"{productPlan.ID} ID ga ega ProductPlanni o'chirdi");

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