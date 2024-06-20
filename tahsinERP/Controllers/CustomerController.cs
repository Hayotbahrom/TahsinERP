using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var customers = db.CUSTOMERS.Where(x => x.IsDeleted == false).ToList();
                return View(customers);
            }
        }

        public ActionResult Details(int? Id)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {

                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var customer = db.CUSTOMERS.Find(Id);
                if (customer == null)
                {
                    return HttpNotFound();
                }
                return View(customer);
            }
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name, Address,DUNS, Type, Country, City, Address, Telephone, Email, ContactPersonName, DirectorName, IsDeleted")] CUSTOMER customer)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        customer.IsDeleted = false;
                        db.CUSTOMERS.Add(customer);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(ex.Message, ex);
                }
                return View(customer);
            }
        }
    }
}