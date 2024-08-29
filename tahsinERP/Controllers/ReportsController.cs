using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ReportsController : Controller
    {
        // GET: Reports
        private string connectionString = ConfigurationManager.AppSettings["SqlConnectionString"];
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult InTransit()
        {
            //using (DBTHSNEntities db = new DBTHSNEntities())
            //{
            //    ViewBag.DataTable = db.Database.SqlQuery("EXEC InTransitView").ToList();
            //}
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("InTransitView", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        ViewBag.DataTable = dataTable;
                        return View();
                    }
                }
            }
        }

        public ActionResult PartRequirement()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("WeeklyPartRequirementByProductPlan", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        ViewBag.DataTable = dataTable;
                        return View();
                    }
                }
            }
        }

        public ActionResult Coverage()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("WeeklyCoverage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@TodayDate", SqlDbType.VarChar, 10);
                    command.Parameters["@TodayDate"].Value = DateTime.Now.ToString("yyyy-MM-dd");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        ViewBag.DataTable = dataTable;
                        return View();
                    }
                }
            }
        }
    }
}