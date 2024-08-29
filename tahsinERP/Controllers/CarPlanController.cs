using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;
using tahsinERP.ViewModels;

namespace tahsinERP.Controllers
{
    public class CarPlanController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                ViewBag.CarList = new SelectList(db.CARS.ToList(), "ID", "OptionCode");
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
                    CAR tempoCar = db.CARS.Where(c => c.ID == plan.CarID).FirstOrDefault();

                    CAR_PLANS newPlan = new CAR_PLANS();
                    newPlan.CarID = plan.CarID;
                    newPlan.Amount = plan.Amount;
                    newPlan.IsDeleted = false;
                    newPlan.StartDate = plan.StartDate;
                    newPlan.DueDate = plan.DueDate;

                    db.CAR_PLANS.Add(newPlan);
                    db.SaveChanges();

                    var userEmail = User.Identity.Name;
                    LogHelper.LogToDatabase(userEmail, "CarPlanController", $"{newPlan.ID} ID ega CarPlan yaratdi");

                    ViewBag.DataTable = db.Database.SqlQuery<GetCarPlanRequirements_Result>("EXEC GetCarPlanRequirements @OptionCode", new SqlParameter("@OptionCode", tempoCar.OptionCode)).ToList();
                    ViewBag.DataTableModel = JsonConvert.SerializeObject(ViewBag.DataTable);
                    ViewBag.IsFileUploaded = true;
                    ViewBag.CarList = new SelectList(db.CARS.ToList(), "ID", "OptionCode");
                }
                return View("Create");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex);
                return View();
            }
        }
        [HttpPost]
        public ActionResult Save(string dataTableModel)
        {
            int dayCount = 0;
            double dayPlanAmount;
            DateTime startDate;
            if (!string.IsNullOrEmpty(dataTableModel))
            {
                //await Task.Run(() =>
                //{
                    var tableModel = JsonConvert.DeserializeObject<DataTable>(dataTableModel);

                    try
                    {
                        using (DBTHSNEntities db = new DBTHSNEntities())
                        {
                            foreach (DataRow row in tableModel.Rows)
                            {
                                string PNo = row["PNo"].ToString();
                                string Requirement = row["Requirement"].ToString();
                                string StartDate = row["StartDate"].ToString();
                                string DueDate = row["DueDate"].ToString();

                                PRODUCTPLAN plan = new PRODUCTPLAN();
                                PRODUCT product = db.PRODUCTS.Where(p => p.PNo.CompareTo(PNo) == 0 && p.IsDeleted == false).FirstOrDefault();

                                plan.ProductID = product.ID;
                                plan.Amount = Convert.ToDouble(Requirement);
                                plan.StartDate = Convert.ToDateTime(StartDate);
                                plan.DueDate = Convert.ToDateTime(DueDate);
                                plan.IsDeleted = false;

                                db.PRODUCTPLANS.Add(plan);
                                db.SaveChanges();

                                var userEmail1 = User.Identity.Name;
                                LogHelper.LogToDatabase(userEmail1, "CarPlanController", $"{plan.ID} ID ega ProductPlan Excel orqali qo'shdi");

                                dayCount = plan.DueDate.Subtract(plan.StartDate).Days;
                                dayPlanAmount = Math.Ceiling(plan.Amount / dayCount);
                                startDate = plan.StartDate;

                                for (int i = 0; i < dayCount; i++)
                                {
                                    PRODUCTPLANS_DAILY dailyPlan = new PRODUCTPLANS_DAILY();
                                    //if (!productPlan.IsTwoShiftPlan)
                                    dailyPlan.DayShift = dayPlanAmount;
                                    //else
                                    //{
                                    //    dailyPlan.DayShift = Math.Ceiling(dayPlanAmount / 2);
                                    //    dailyPlan.NightShift = dayPlanAmount - dailyPlan.DayShift;
                                    //}
                                    dailyPlan.PlanID = plan.ID;
                                    dailyPlan.Day = startDate;
                                    startDate = startDate.AddDays(1);
                                    db.PRODUCTPLANS_DAILY.Add(dailyPlan);
                                }

                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                //});
            }
            return RedirectToAction("Index");
        }
    }
}