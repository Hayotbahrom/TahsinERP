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
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("InTransitView", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@TodayDate", SqlDbType.VarChar, 10);
                    command.Parameters["@TodayDate"].Value = "2024-08-01";

                    command.Parameters.Add("@EndDate", SqlDbType.VarChar, 10);
                    command.Parameters["@EndDate"].Value = "2024-12-31";

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
                using (SqlCommand command = new SqlCommand("WeeklyPartRequirement", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@TodayDate", SqlDbType.VarChar, 10);
                    command.Parameters["@TodayDate"].Value = "2024-08-01";

                    command.Parameters.Add("@EndDate", SqlDbType.VarChar, 10);
                    command.Parameters["@EndDate"].Value = "2024-12-31";


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
                    command.Parameters["@TodayDate"].Value = "2024-08-26";

                    command.Parameters.Add("@EndDate", SqlDbType.VarChar, 10);
                    command.Parameters["@EndDate"].Value = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");

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