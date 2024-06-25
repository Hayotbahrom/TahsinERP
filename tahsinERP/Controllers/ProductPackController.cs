using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ProductPackController : Controller
    {
        // GET: ProductPack
        public ActionResult Index()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                var productpack = db.PRODUCTPACKS
                    .Where(x => x.IsDeleted == false)
                    .Include(p =>p.PRODUCT)
                    .ToList();
                return View(productpack);
            }
        }
        private DBTHSNEntities db = new DBTHSNEntities();
        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // ViewBag orqali "Product" ma'lumotlari
                var products = db.PRODUCTS.ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "Name"); // Use the correct property name here

                return View();
            }
        }

        [HttpPost]
        public ActionResult Create(PRODUCTPACK model, int ProductID)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    model.ProdID = ProductID;
                    model.RegDate = DateTime.Now;
                    model.IsDeleted = false;

                    db.PRODUCTPACKS.Add(model);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }

                var products = db.PRODUCTS.ToList();
                ViewBag.ProductList = new SelectList(products, "ID", "Name", model.ProdID); // Use the correct property name here

                return View(model);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    PRODUCTPACK productPack = db.PRODUCTPACKS
                                                .Include(p => p.PRODUCT)
                                                .FirstOrDefault(p => p.ID == id);

                    if (productPack == null)
                    {
                        return HttpNotFound();
                    }

                    var products = db.PRODUCTS.ToList();
                    ViewBag.ProductList = new SelectList(products, "ID", "Name");

                    return View(productPack);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error occurred while fetching data: " + ex.Message;
                return View("Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,ProdID,PrPackMaterial,PrPackQty,Securement,Dunnage,PrWeight,PrLength,PrWidth,PrHeight,ScPackMaterial,ScWeight,ScLength,ScWidth,ScHeight,ScPackQty,PalletType,PltLength,PltWidth,PltHeight,PltWeight,RegDate,IsActive,IsDeleted")] PRODUCTPACK productPack)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        // ProdID orqali Product ma'lumotlarini olib, productPack obyektiga qo'shib qo'yish
                        productPack.PRODUCT = db.PRODUCTS.FirstOrDefault(p => p.ID == productPack.ProdID);

                        productPack.IsDeleted = false;
                        productPack.RegDate = DateTime.Now;
                        db.Entry(productPack).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }

                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var products = db.PRODUCTS.ToList();
                    ViewBag.ProductList = new SelectList(products, "ID", "Name");
                }
                return View(productPack);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error occurred while saving data: " + ex.Message;
                return View("Error");
            }
        }


    }
}