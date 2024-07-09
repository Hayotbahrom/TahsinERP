using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Design;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class SContractController : Controller
    {
        // GET: SContract | Index
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                List<S_CONTRACTS> list = db.S_CONTRACTS
                                           .Include(sc => sc.CUSTOMER)
                                           .Where(sc => sc.IsDeleted == false)
                                           .ToList();

                return View(list);
            }
        }
        // __________



        // Create
        public ActionResult Create()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                SContractViewModel SContractViewModel = new SContractViewModel();
                ViewBag.Customers = new SelectList(db.CUSTOMERS.Where(c => c.IsDeleted == false).ToList(), "ID", "Name");
                ViewBag.Products = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "PNo");
                return View(SContractViewModel);
            }
        }

        [HttpPost]
        public ActionResult Create(SContractViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Model cannot be null");
            }

            if (model.Products == null || !model.Products.Any())
            {
                ModelState.AddModelError("Products", "At least one Product is required.");
                return View(model);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // Add New S_CONTRACT
                S_CONTRACTS newContract = new S_CONTRACTS
                {
                    ContractNo = model.ContractNo,
                    IssuedDate = DateTime.Now,
                    CompanyID = int.Parse(ConfigurationManager.AppSettings["companyID"]),
                    CustomerID = model.CustomerID,
                    Currency = model.Currency,
                    Amount = model.Amount,
                    Incoterms = model.Incoterms,
                    PaymentTerms = model.PaymentTerms,
                    DueDate = model.DueDate,
                    IsDeleted = false,
                };

                db.S_CONTRACTS.Add(newContract);
                db.SaveChanges();

                // Yangi Contract ning ID sini olish
                int newContractID = newContract.ID;

                // Yangi Product lar ni saqlash
                foreach (var item in model.Products)
                {
                    if(item == null)
                    {
                        ModelState.AddModelError("Products", "Product cannot be null");
                        return View(model);
                    }

                    S_CONTRACT_PRODUCTS newProduct = new S_CONTRACT_PRODUCTS
                    {
                        ContractID = newContractID,
                        ProductID = item.ProductID,
                        PiecePrice = item.PiecePrice,
                        Unit = item.Unit,
                        Amount = item.Amount
                    };

                    db.S_CONTRACT_PRODUCTS.Add(newProduct);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        // __________



        // Details
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contract = db.S_CONTRACTS
                                 .Include(c => c.CUSTOMER)
                                 .FirstOrDefault(p => p.ID == id);

                if (contract == null)
                {
                    return HttpNotFound();
                }

                var productList = db.S_CONTRACT_PRODUCTS
                                    .Include(pl => pl.PRODUCT)
                                    .Where(pl => pl.ContractID == contract.ID)
                                    .ToList();

                ViewBag.ProductList = productList;
                return View(contract);
            }
        }
        // __________



        // Main Edit
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contract = db.S_CONTRACTS.Find(id);

                if (contract == null)
                {
                    return HttpNotFound();
                }


                ViewBag.Customer = new SelectList(db.CUSTOMERS, "ID", "Name", contract.CustomerID);
                ViewBag.ProductList = db.S_CONTRACT_PRODUCTS.Where(sp => sp.ContractID == contract.ID).ToList();

                db.Entry(contract).Reference(c => c.CUSTOMER).Load();
                return View(contract);
            }
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,ContractNo,IssuedDate,CompanyID,CustomerID,Currency,Amount,Incoterms,PaymentTerms,DueDate,IsDeleted")] S_CONTRACTS contract)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        contract.IsDeleted = false;
                        db.Entry(contract).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }
                }

                ViewBag.Customer = new SelectList(db.CUSTOMERS, "ID", "Name", contract.CustomerID);
                ViewBag.ProductList = db.S_CONTRACT_PRODUCTS.Where(sp => sp.ContractID == contract.ID).ToList();
                return View(contract);
            }
        }
        // __________



        // Edit Product
        [HttpGet]
        public ActionResult EditProduct(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contractProduct = db.S_CONTRACT_PRODUCTS.Find(id);
                if (contractProduct == null)
                {
                    return HttpNotFound();
                }

                var allProducts = db.PRODUCTS.Select(p => new SelectListItem
                {
                    Value = p.ID.ToString(),
                    Text = p.PNo
                }).ToList();

                ViewBag.ProductList = allProducts;
                return View(contractProduct);
            }
        }

        [HttpPost]
        public ActionResult EditProduct([Bind(Include = "ID,ProductID,PiecePrice,Unit,Amount")] S_CONTRACT_PRODUCTS contractProduct)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        db.Entry(contractProduct).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }
                }

                ViewBag.ContractList = db.S_CONTRACT_PRODUCTS.Where(cp => cp.ContractID == contractProduct.ContractID).ToList();
                ViewBag.ProductList = db.S_CONTRACT_PRODUCTS.Where(p => p.ProductID == contractProduct.ProductID).ToList();
                return View(contractProduct);
            }
        }
        // __________



        // Delete Main
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contractProduct = db.S_CONTRACTS.Find(id);
                if (contractProduct == null)
                {
                    return HttpNotFound();
                }

                ViewBag.ProductList = db.S_CONTRACT_PRODUCTS.Where(pl => pl.ContractID == contractProduct.ID).ToList();
                db.Entry(contractProduct).Reference(c => c.CUSTOMER).Load();
                return View(contractProduct);
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id, FormCollection gfs)
        {
            if (!ModelState.IsValid || id == null)
            {
                return View();
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                S_CONTRACTS contractToDelete = db.S_CONTRACTS.Find(id);
                if (contractToDelete != null)
                {
                    contractToDelete.IsDeleted = true;

                    try
                    {
                        db.Entry(contractToDelete).State = EntityState.Modified;
                        var contractProducts = db.S_CONTRACT_PRODUCTS.Where(sp => sp.ContractID == contractToDelete.ID).ToList();

                        foreach (var contractProduct in contractProducts)
                        {
                            db.S_CONTRACT_PRODUCTS.Remove(contractProduct);
                        }

                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "O'zgarishlarni saqlab bo'madi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bunday shartnoma topilmadi.");
                }
            }
            
            return View();
        }



        // Delete Contract-Product
        [HttpDelete]
        public ActionResult DeleteProduct(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                S_CONTRACT_PRODUCTS contractProductToDelete = db.S_CONTRACT_PRODUCTS.Find(id);

                if (ModelState.IsValid)
                {
                    if (contractProductToDelete != null)
                    {
                        try
                        {
                            db.S_CONTRACT_PRODUCTS.Remove(contractProductToDelete);
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "O'zgarishlarni saqlab bo'lmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bunday shartnoma topilmadi.");
                    }
                }

                return View(contractProductToDelete);
            }
        }
        // __________
    }
}