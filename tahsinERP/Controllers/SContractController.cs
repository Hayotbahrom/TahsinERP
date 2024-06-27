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
        private DBTHSNEntities db = new DBTHSNEntities();
        // GET: SContract | Index
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                List<S_CONTRACTS> list = db.S_CONTRACTS.Where(sc => sc.IsDeleted == false).ToList();
                return View(list);
            }
        }
        // __________



        // Create
        public ActionResult Create()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {
                S_CONTRACTS contract = new S_CONTRACTS();
                ViewBag.Customers = new SelectList(db.CUSTOMERS.Where(c => c.IsDeleted == false).ToList(), "ID", "Name");
                return View(contract);
            }
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "ID,ContractNo,IssuedDate,CompanyID,CustomerID,Currency,Amount,Incoterms,PaymentTerms,DueDate,IsDeleted")] S_CONTRACTS contract, int customerId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var companyId = ConfigurationManager.AppSettings["companyID"];
                    contract.CompanyID = int.Parse(companyId);
                    contract.CustomerID = customerId;
                    contract.IsDeleted = false;

                    db.S_CONTRACTS.Add(contract);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
            }

            ViewBag.Customer = new SelectList(db.CUSTOMERS.Where(cs => cs.IsDeleted == false).ToList());
            return View(contract);
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
                var contract = db.S_CONTRACTS.Find(id);
                if (contract == null)
                {
                    return HttpNotFound();
                }

                ViewBag.ProductList = db.S_CONTRACT_PRODUCTS.Where(sc => sc.ContractID == contract.ID).ToList();
                return View(contract);
            }
        }
        // __________


        // Main Edit
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

        [HttpPost]
        public ActionResult Delete(int id, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
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
            }

            return View();
        }
        // __________



        // Delete Contract-Product
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