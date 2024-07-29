using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;
using System.Web.Security;

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
                ViewBag.Units = new SelectList(db.UNITS.ToList(), "ID", "ShortName");
                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "SContractController", "Create[Get]");
                return View(SContractViewModel);
            }
        }

        [HttpPost]
        public ActionResult Create(SContractViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Shartnoma bekor bo'lishi mumkin emas");
            }

            if (model.ProductList == null || !model.ProductList.Any())
            {
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
                    //CustomerID = 2,
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
                        ModelState.AddModelError("Products", "Mahsulot bo'sh boʻlishi mumkin emas");
                        ViewBag.Customers = new SelectList(db.CUSTOMERS.Where(c => c.IsDeleted == false).ToList(), "ID", "Name");
                        ViewBag.Products = new SelectList(db.PRODUCTS.Where(p => p.IsDeleted == false).ToList(), "ID", "PNo");
                        return View(model);
                    }

                    S_CONTRACT_PRODUCTS newProduct = new S_CONTRACT_PRODUCTS
                    {
                        ContractID = newContractID,
                        ProductID = item.ProductID,
                        PiecePrice = (int)item.PiecePrice,
                        UnitID = item.UnitID,
                        Amount = item.Amount
                    };

                    db.S_CONTRACT_PRODUCTS.Add(newProduct);
                }

                db.SaveChanges();

                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "SContractController", "Create[Post]");

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
                                    .Include(pl => pl.UNIT)
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
                var contract = db.S_CONTRACTS.Include(x => x.S_CONTRACT_PRODUCTS).Where(x => x.ID == id && x.IsDeleted == false).FirstOrDefault();
                if (contract == null)
                {
                    return HttpNotFound();
                }

                var model = new SContractViewModel
                {
                    ContractNo = contract.ContractNo,
                    CustomerID = contract.CustomerID,
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
                        ProductPNo = p.PRODUCT.PNo,
                        PiecePrice = (int)p.PiecePrice,
                        UnitID = p.UnitID,
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
                ViewBag.Units = new SelectList(db.UNITS.ToList(), "ID", "ShortName");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SContractViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Model bo'sh bo'lishi mumkin emas");
            }

            if (model.ProductList == null || !model.ProductList.Any())
            {
                ModelState.AddModelError("Products", "Kamida bitta mahsulot kerak.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var contractToUpdate = db.S_CONTRACTS.Find(model.ID);
                    if (contractToUpdate == null)
                    {
                        return HttpNotFound();
                    }

                    contractToUpdate.ContractNo = model.ContractNo;
                    contractToUpdate.CustomerID = model.CustomerID;
                    contractToUpdate.IssuedDate = model.IssuedDate;
                    contractToUpdate.DueDate = model.DueDate;
                    contractToUpdate.Incoterms = model.Incoterms;
                    contractToUpdate.PaymentTerms = model.PaymentTerms;
                    contractToUpdate.Currency = model.Currency;
                    contractToUpdate.Amount = (int)model.Amount;


                    db.Entry(contractToUpdate).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (RetryLimitExceededException)
                    {
                        ModelState.AddModelError("", "Kontrakt o'zgarishlarini saqlab bo'lmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                    }


                    // Update ProductList
                    foreach (var product in model.ProductList)
                    {
                        var existingProduct = db.S_CONTRACT_PRODUCTS.Find(product.ID);
                        if (existingProduct != null)
                        {
                            existingProduct.PiecePrice = product.PiecePrice;
                            existingProduct.UnitID = product.UnitID;
                            existingProduct.Amount = product.Amount;

                            db.Entry(existingProduct).State = EntityState.Modified;
                        }
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (RetryLimitExceededException)
                    {
                        ModelState.AddModelError("", "Maxsulot o'zgarishlarini saqlab bo'lmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratsiyasiga muroaat qiling.");
                    }
                    return RedirectToAction("Index");
                }
            }

            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "SContractController", "Edit[Post]");

            ViewBag.ProductList = model.ProductList;
            return View(model);
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
                var contract = db.S_CONTRACTS.Find(id);
                if (contract == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    ViewBag.ProductList = db.S_CONTRACT_PRODUCTS
                                            .Include(pl => pl.PRODUCT)
                                            .Include(pl => pl.UNIT)
                                            .Where(pl => pl.ContractID == contract.ID)
                                            .ToList();
                }

                db.Entry(contract).Reference(c => c.CUSTOMER).Load();
                return View(contract);
            }
        }

        [HttpPost]
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

                        //db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bunday shartnoma topilmadi.");
                }
            }

            var userEmail = User.Identity.Name;
            LogHelper.LogToDatabase(userEmail, "SContractController", "Delete[Post]");

            return View();
        }
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

                var userEmail = User.Identity.Name;
                LogHelper.LogToDatabase(userEmail, "SContractController", "DeleteProduct[Post]");
                return View(contractProductToDelete);
            }
        }
    }
}