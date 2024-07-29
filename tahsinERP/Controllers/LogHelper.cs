using DocumentFormat.OpenXml.EMMA;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
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
                USERLOG log = new USERLOG
                {
                    ControllerName = controllerName,
                    ActionName = actionName,
                    DateTime = DateTime.Now,
                    UserID = GetUserId(userEmail)
                };

                db.USERLOGS.Add(log);
                try
                {
                    db.SaveChanges();
                }
                catch (RetryLimitExceededException)
                {
                    Console.WriteLine("Error in LogHelper");
                }
            }
        }

        private static int GetUserId(string userEmail)
        {
            using (var db = new DBTHSNEntities())
            {
                USER getUser = db.USERS.Where(u => u.IsDeleted == false && u.Email.Equals(userEmail)).FirstOrDefault();
                return getUser.ID;
            }
        }
    }
}
