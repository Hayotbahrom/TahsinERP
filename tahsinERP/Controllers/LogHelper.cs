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
                        UserID = GetUserId(userEmail),
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

        private static int GetUserId(string userEmail)
        {
            using (var db = new DBTHSNEntities())
            {
                USER getUser = db.USERS
                                 .Where(u => u.IsDeleted == false && u.Email.Equals(userEmail))
                                 .FirstOrDefault();

                if (getUser != null)
                {
                    return getUser.ID;
                }
                else
                {
                    throw new Exception("User not found");
                }
            }
        }

    }
}
