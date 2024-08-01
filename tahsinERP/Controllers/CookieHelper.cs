using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public static class CookieHelper
    {
        public static bool IsConfirmed(string userEmail)
        {
            var UserID = LogHelper.GetUserId(userEmail);
            var MacAddress = NetworkHelper.GetMacAddress(NetworkHelper.GetIpAddress());

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var confirmation = db.COOKIES_CONFIRMATION.Where(c => c.UserID == UserID).FirstOrDefault();

                if (confirmation == null)
                {
                    return false;
                }
                    
                return confirmation.MacAdress == MacAddress ? true : false;
            }
        }

        public static void Confirm(string userEmail)
        {

            using(DBTHSNEntities dB = new DBTHSNEntities())
            {
                var UserID = LogHelper.GetUserId(userEmail);

                var cookieConfirm = new COOKIES_CONFIRMATION
                {
                    UserID = UserID,
                    IssuedDT = DateTime.Now,
                    MacAdress = NetworkHelper.GetMacAddress(NetworkHelper.GetIpAddress()),
                    Confirmation = true
                };

                dB.COOKIES_CONFIRMATION.Add(cookieConfirm);
                dB.SaveChanges();
            }
        }
    }
}
