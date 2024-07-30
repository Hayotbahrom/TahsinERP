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
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var productpack = db.PRODUCTPACKS
                    .Where(x => x.IsDeleted == false)
                    .Include(p => p.PRODUCT)
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
                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "ProductPackController", "Create[Post]");
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
                    ViewBag.ProductList = new SelectList(products, "ID", "Name", productPack.ProdID);

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
        public ActionResult Edit([Bind(Include = "ID,ProdID,PrPackMaterial,PrPackQty,Securement,Dunnage,PrWeight,PrLength,PrWidth,PrHeight,ScPackMaterial,ScWeight,ScLength,ScWidth,ScHeight,ScPackQty,PalletType,PltLength,PltWidth,PltHeight,PltWeight,RegDate,IsActive,IsDeleted")] PRODUCTPACK productPack, int productId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (DBTHSNEntities db = new DBTHSNEntities())
                    {
                        productPack.IsDeleted = false;
                        productPack.RegDate = DateTime.Now;
                        productPack.ProdID = productId;

                        db.Entry(productPack).State = EntityState.Modified;
                        db.Entry(productPack).Property(p => p.ProdID).IsModified = true; // ProdID ni modified deb belgilash
                        db.SaveChanges();
                    }
                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "ProductPackController", "Edit[Post]");
                    return RedirectToAction("Index");
                }

                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var products = db.PRODUCTS.ToList();
                    ViewBag.ProductList = new SelectList(products, "ID", "Name", productPack.ProdID);
                }
                return View(productPack);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error occurred while saving data: " + ex.Message;
                return View("Error");
            }
        }
        public ActionResult Details(int? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                try
                {
                    var productPack = db.PRODUCTPACKS.Include(pr => pr.PRODUCT).FirstOrDefault(p => p.ID == id);
                    if (productPack == null)
                    {
                        return HttpNotFound();
                    }
                    return View(productPack );
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Malumotni olishda hatolik yuz berdi!");

                }

            }
            return View();
        }

        public ActionResult Delete(int? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                try
                {
                    var productPack = db.PRODUCTPACKS.Include(pr => pr.PRODUCT).FirstOrDefault(p => p.ID == id);
                    if (productPack == null)
                    {
                        return HttpNotFound();
                    }
                    return View(productPack);
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Malumotni olishda hatolik yuz berdi!");

                }

            }
            return View();
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                PRODUCTPACK prodPack = db.PRODUCTPACKS.Find(id);
                if (prodPack != null)
                {
                    prodPack.IsDeleted = true;
                    db.Entry(prodPack).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "ProductPackController", "Edit[Post]");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}