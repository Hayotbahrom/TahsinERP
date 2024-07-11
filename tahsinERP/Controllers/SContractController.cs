using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
            using (DBTHSNEntities db = new DBTHSNEntities())
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

            if (model.ProductList == null || !model.ProductList.Any())
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
                    Amount = (int)model.Amount,
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
                foreach (var item in model.ProductList)
                {
                    if (item == null)
                    {
                        ModelState.AddModelError("Products", "Product cannot be null");
                        return View(model);
                    }

                    S_CONTRACT_PRODUCTS newProduct = new S_CONTRACT_PRODUCTS
                    {
                        ContractID = newContractID,
                        ProductID = item.ProductID,
                        PiecePrice = (int)item.PiecePrice,
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
        public ActionResult Edit(int id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var contract = db.S_CONTRACTS.Find(id);
                if (contract == null)
                {
                    return HttpNotFound();
                }

                var model = new SContractViewModel
                {
                    ID = contract.ID,
                    ContractNo = contract.ContractNo,
                    CustomerID = contract.CustomerID,
                    IssuedDate = contract.IssuedDate,
                    DueDate = contract.DueDate,
                    Incoterms = contract.Incoterms,
                    PaymentTerms = contract.PaymentTerms,
                    Currency = contract.Currency,
                    Amount = (int)contract.Amount,
                    ProductList = contract.S_CONTRACT_PRODUCTS.Select(p => new SContractProductViewModel
                    {
                        ID = p.ID,
                        SContractID = p.ContractID,
                        ProductID = p.ProductID,
                        PiecePrice = (int)p.PiecePrice,
                        Unit = p.Unit,
                        Amount = (int)p.Amount,
                        PRODUCT = new ProductViewModel
                        {
                            ID = p.PRODUCT.ID,
                            PNo = p.PRODUCT.PNo,
                            Name = p.PRODUCT.Name
                        }
                    }).ToList()
                };

                ViewBag.Customers = new SelectList(db.CUSTOMERS.Where(x => x.IsDeleted == false).ToList(), "ID", "Name", contract.CustomerID);
                ViewBag.ProductList = model.ProductList;

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SContractViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Model cannot be null");
            }

            if (model.ProductList == null || !model.ProductList.Any())
            {
                ModelState.AddModelError("Products", "At least one Product is required.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var contract = db.S_CONTRACTS.Find(model.ID);
                    if (contract == null)
                    {
                        return HttpNotFound();
                    }

                    contract.ContractNo = model.ContractNo;
                    contract.CustomerID = model.CustomerID;
                    contract.IssuedDate = model.IssuedDate;
                    contract.DueDate = model.DueDate;
                    contract.Incoterms = model.Incoterms;
                    contract.PaymentTerms = model.PaymentTerms;
                    contract.Currency = model.Currency;
                    contract.Amount = (int)model.Amount;

                    db.Entry(contract).State = EntityState.Modified;
                    db.SaveChanges();

                    // Update product list
                    foreach (var product in model.ProductList)
                    {
                        var a = product.ProductID;

                        var existingProduct = db.S_CONTRACT_PRODUCTS.Find(product.ProductID);
                        if (existingProduct != null)
                        {
                            existingProduct.PiecePrice = (int)product.PiecePrice;
                            existingProduct.Unit = product.Unit;
                            existingProduct.Amount = product.Amount;
                            db.Entry(existingProduct).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            ViewBag.ProductList = model.ProductList;
            return View(model);
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
        public ActionResult Delete(int? id, FormCollection gfs)
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
    }
}