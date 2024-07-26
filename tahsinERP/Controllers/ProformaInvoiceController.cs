using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class ProformaInvoiceController : Controller
    {
        // GET: ProformaInvoice
        public async Task<ActionResult> Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var list = await db.P_PROFORMA_INVOICES
                    .Where(x => x.IsDeleted == false)
                    .Include(x => x.P_INVOICES)
                    .ToListAsync();

                return View(list);
            }
        }
        public async Task<ActionResult> Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.Supplier = new SelectList(await db.SUPPLIERS.Where(x => x.IsDeleted == false).ToListAsync(), "ID", "Name");
                ViewBag.PInvoices = new SelectList(await db.P_INVOICES.Where(x => x.IsDeleted == false).ToListAsync(), "ID", "InvoiceNo");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(P_PROFORMA_INVOICES model)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                P_PROFORMA_INVOICES invoice = new P_PROFORMA_INVOICES()
                {
                    InvoiceID = model.InvoiceID,
                    SupplierID = model.SupplierID,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    InvoiceDate = model.InvoiceDate,
                    CompanyID = 1,
                    IsDeleted = false
                };

                db.P_PROFORMA_INVOICES.Add(invoice);
                await db.SaveChangesAsync();

                ViewBag.Supplier = new SelectList(await db.SUPPLIERS.Where(x => x.IsDeleted == false).ToListAsync(), "ID", "Name");
                ViewBag.PInvoices = new SelectList(await db.P_INVOICES.Where(x => x.IsDeleted == false).ToListAsync(), "ID", "InvoiceNo");

                return RedirectToAction("Index");
            }
        }
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var  invoice = await db.P_PROFORMA_INVOICES
                    .Include(x => x.P_INVOICES)
                    .Where(x => x.ID == id)
                    .FirstOrDefaultAsync();

                if (invoice == null)
                    return HttpNotFound();
 
                return View(invoice);
            }
        }
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var invoice = await db.P_PROFORMA_INVOICES
                    .Include(x => x.P_INVOICES)
                    .Where(x => x.ID == id)
                    .FirstOrDefaultAsync();

                if (invoice == null)
                    return HttpNotFound();

                return View(invoice);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? ID, FormCollection gfs)
        {
            if (ModelState.IsValid)
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    var invoiceToDelete = await db.P_PROFORMA_INVOICES.FindAsync(ID);
                    if (invoiceToDelete != null)
                    {
                        invoiceToDelete.IsDeleted = true;
                        try
                        {
                            await db.SaveChangesAsync();
                            return RedirectToAction("Index");
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bunday invoice topilmadi.");
                    }
                }
            }
            return View();
        }
        public async Task<ActionResult> Edit(int? ID)
        {
            if (ID == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var invoice = await db.P_PROFORMA_INVOICES
                    .Include(x => x.P_INVOICES)
                    .Where(x => x.ID == ID)
                    .FirstOrDefaultAsync();

                if (invoice == null)
                    return HttpNotFound();

                ViewBag.Supplier = new SelectList(await db.SUPPLIERS.Where(x => x.IsDeleted == false).ToListAsync(), "ID", "Name", invoice.SupplierID); 
                ViewBag.PInvoices = new SelectList(await db.P_INVOICES.Where(x => x.IsDeleted == false).ToListAsync(), "ID", "OrderNo", invoice.InvoiceID);

                return View(invoice);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(P_PROFORMA_INVOICES invoice)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    var  invoiceToUpdate = await db.P_PROFORMA_INVOICES.FindAsync(invoice.ID);
                    if (invoiceToUpdate != null)    
                    {
                        invoiceToUpdate.PrInvoiceNo = invoice.PrInvoiceNo;
                        invoiceToUpdate.SupplierID = invoice.SupplierID;
                        invoiceToUpdate.InvoiceID = invoice.InvoiceID;
                        invoiceToUpdate.InvoiceDate = invoice.InvoiceDate;
                        invoiceToUpdate.Currency = invoice.Currency;
                        invoiceToUpdate.Amount = invoice.Amount;
                        invoiceToUpdate.IsDeleted = false;

                        db.Entry(invoiceToUpdate).State = EntityState.Modified;

                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException?.InnerException is SqlException sqlEx)
                            {
                                if (sqlEx.Number == 547) // Foreign key constraint violation
                                {
                                    ModelState.AddModelError("", "The OrderID does not exist in the P_ORDERS table.");
                                }
                                else
                                {
                                    ModelState.AddModelError("", $"Database update error: {sqlEx.Message}");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                            }
                        }
                        catch (RetryLimitExceededException)
                        {
                            ModelState.AddModelError("", "Oʻzgarishlarni saqlab boʻlmadi. Qayta urinib ko'ring va agar muammo davom etsa, tizim administratoriga murojaat qiling.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invoice not found.");
                    }
                }

                // Re-populate dropdown lists in case of an error
                ViewBag.Supplier = new SelectList(await db.SUPPLIERS.ToListAsync(), "ID", "Name", invoice.SupplierID);
                ViewBag.PInvoices = new SelectList(await db.P_INVOICES.ToListAsync(), "ID", "InvoiceNo", invoice.InvoiceID);

                return View(invoice);
            }
        }
    }
}