using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ProductController : Controller
    {
        private DBTHSNEntities db = new DBTHSNEntities();
        private string[] sources = new string[3] { "", "Import", "Local" }; // Corrected "Lokal" to "Local"

        private byte[] avatar;
        private int productPhotoMaxLength = Convert.ToInt32(ConfigurationManager.AppSettings["photoMaxSize"]);
        public ActionResult Index(string type)
        {
            List<PRODUCT> list = db.PRODUCTS.Where(p => p.IsDeleted == false).ToList();
            ViewBag.SourceList = new SelectList(sources, type);
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.SourceList = new SelectList(sources);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PNo, Name, Type, Weight, Length, Width, Height, Unit, Description, PNo2, PNo3, PNo4, PackID, IsDeleted")] PRODUCT product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set IsDeleted to false and save the product to get the ID
                    product.IsDeleted = false;
                    db.PRODUCTS.Add(product);
                    db.SaveChanges();

                    // Handle image upload
                    var imageFile = Request.Files["productPhotoUpload"]; // Ensure name matches
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        if (imageFile.ContentLength < productPhotoMaxLength)
                        {
                            var photoImage = new PRODUCTIMAGE
                            {
                                ProdID = product.ID, // Use the product ID
                                Image = new byte[imageFile.ContentLength],
                                IsDeleted = false

                            };

                            imageFile.InputStream.Read(photoImage.Image, 0, photoImage.Image.Length);

                            db.PRODUCTIMAGES.Add(photoImage);
                            db.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("", "Unable to load photo, it's more than 2MB. Try again, and if the problem persists, see your system administrator.");
                            throw new RetryLimitExceededException();
                        }
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
            }

            ViewBag.SourceList = new SelectList(sources);
            return View(product);
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var product = db.PRODUCTS.Find(id);
            if (product == null)
                return HttpNotFound();

            // Retrieve the product image
            var productImage = db.PRODUCTIMAGES.FirstOrDefault(img => img.ProdID == id);
            if (productImage != null)
            {
                ViewBag.ProductImage = Convert.ToBase64String(productImage.Image);
            }

            return View(product);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.PRODUCTS.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Type = new SelectList(sources, product.Type);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PRODUCT product)
        {
            if (ModelState.IsValid)
            {
                var productToUpdate = db.PRODUCTS.Find(product.ID);
                if (productToUpdate != null)
                {
                    productToUpdate.PNo = product.PNo;
                    productToUpdate.Name = product.Name;
                    productToUpdate.Description = product.Description;
                    productToUpdate.Weight = product.Weight;
                    productToUpdate.Length = product.Length;
                    productToUpdate.Width = product.Width;
                    productToUpdate.Height = product.Height;
                    productToUpdate.Unit = product.Unit;
                    productToUpdate.Type = product.Type;
                    productToUpdate.PNo2 = product.PNo2;
                    productToUpdate.PNo3 = product.PNo3;
                    productToUpdate.PNo4 = product.PNo4;
                    productToUpdate.PackID = product.PackID;

                    var imageFile = Request.Files["productPhotoUpload"]; // Ensure name matches
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        if (imageFile.ContentLength < productPhotoMaxLength)
                        {
                            // Check for existing image
                            var existingImage = db.PRODUCTIMAGES.FirstOrDefault(pi => pi.ProdID == product.ID);

                            if (existingImage != null)
                            {
                                // Update existing image
                                existingImage.Image = new byte[imageFile.ContentLength];
                                imageFile.InputStream.Read(existingImage.Image, 0, existingImage.Image.Length);
                            }
                            else
                            {
                                // Add new image
                                var photoImage = new PRODUCTIMAGE
                                {
                                    ProdID = product.ID, // Use the product ID
                                    Image = new byte[imageFile.ContentLength],
                                    IsDeleted = false
                                };

                                imageFile.InputStream.Read(photoImage.Image, 0, photoImage.Image.Length);
                                db.PRODUCTIMAGES.Add(photoImage);
                            }

                            db.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("", "Unable to load photo, it's more than 2MB. Try again, and if the problem persists, see your system administrator.");
                            throw new RetryLimitExceededException();
                        }
                    }

                    db.Entry(productToUpdate).State = (System.Data.Entity.EntityState)EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(productToUpdate);
            }
            return View(product);
        }




        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.PRODUCTS.Find(Id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
            //return RedirectToAction("SupplierParts?supplierId="+supplier.ID);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                PRODUCT productToUpdate = db.PRODUCTS.Find(ID);
                if (productToUpdate != null)
                {

                    productToUpdate.IsDeleted = true;
                    if (TryUpdateModel(productToUpdate, "", new string[] { "IsDeleted" }))
                    {
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                        }
                    }
                }
            }

            return View();
        }
    }
}
