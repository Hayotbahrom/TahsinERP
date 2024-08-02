using DocumentFormat.OpenXml.EMMA;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public static class LogHelper
    {
        public static void LogToDatabase(string userEmail, string controllerName, string actionName)
        {
            using (var db = new DBTHSNEntities())
            {
                try
                {
                    USERLOG log = new USERLOG
                    {
                        ControllerName = controllerName,
                        ActionName = actionName,
                        DateTime = DateTime.Now,
                        UserID = UserHelper.GetUserId(userEmail),
                        IP = NetworkHelper.GetIpAddress(),
                        MacAddr = NetworkHelper.GetMacAddress(NetworkHelper.GetIpAddress())
                    };

                    db.USERLOGS.Add(log);
                    db.SaveChanges(); // Save changes to the database
                }
                catch (RetryLimitExceededException)
                {
                    Console.WriteLine("Error in LogHelper");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
