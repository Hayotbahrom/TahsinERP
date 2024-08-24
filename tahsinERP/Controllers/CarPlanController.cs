using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class CarPlanController : Controller
    {
        // GET: CarPlan
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var carList = db.CARS.Select(c => new { c.ID, c.Model }).Distinct().ToList();
                ViewBag.CarList = new SelectList(carList, "ID", "Model");

                return View();
            }
        }   



        [HttpPost]
        public ActionResult Create(CAR_PLANS plan)
        {
            try
            {
                using (DBTHSNEntities db = new DBTHSNEntities())
                {
                    CAR_PLANS newPlan = new CAR_PLANS();
                    newPlan.CarID = plan.CarID;
                    newPlan.Amount = plan.Amount;
                    newPlan.IsDeleted = false;
                    newPlan.StartDate = plan.StartDate;
                    newPlan.DueDate = plan.DueDate;

                    db.CAR_PLANS.Add(newPlan);
                    db.SaveChanges();

                    ViewBag.PlanUploaded = true;
                    ViewBag.CarList = new SelectList(db.CARS.ToList(), "ID", "OptionCode");
                    return View("Create");
                }
            } catch(Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
                return View();
            }
        }
    }
}