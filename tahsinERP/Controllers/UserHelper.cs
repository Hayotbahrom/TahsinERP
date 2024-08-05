using System;
using System.Linq;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public static class UserHelper
    {
        public static int GetUserId(string userEmail)
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

        public static string GetFullName(string userEmail = null, int? userID = null)
        {
            if (userEmail == null && userID == null)
            {
                throw new ArgumentException("Email va UserID ham bir vaqtda NULL bo'lishi mumkin emas");
            }

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                USER getUser = db.USERS
                                 .Where(u => u.IsDeleted == false &&
                                             ((userEmail != null && u.Email.Equals(userEmail)) ||
                                              (userID != null && u.ID == userID)))
                                 .FirstOrDefault();

                if (getUser != null)
                {
                    return getUser.FullName;
                }
                else
                {
                    throw new Exception("User not found");
                }
            }
        }
    }
}
