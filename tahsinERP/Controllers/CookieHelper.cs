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
            var UserID = UserHelper.GetUserId(userEmail);
            var MacAddress = NetworkHelper.GetMacAddress(NetworkHelper.GetIpAddress());
            var IP = NetworkHelper.GetIpAddress();

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var confirmation = db.COOKIES_CONFIRMATION.Where(c => c.UserID == UserID && c.MacAddress == MacAddress && c.IPAddress == IP).FirstOrDefault();

                if (confirmation == null)
                {
                    return false;
                }
                else
                    return true;
            }
        }

        public static void Confirm(string userEmail)
        {

            using (DBTHSNEntities dB = new DBTHSNEntities())
            {
                var UserID = UserHelper.GetUserId(userEmail);

                var cookieConfirm = new COOKIES_CONFIRMATION
                {
                    UserID = UserID,
                    IssuedDT = DateTime.Now,
                    MacAddress = NetworkHelper.GetMacAddress(NetworkHelper.GetIpAddress()),
                    IPAddress = NetworkHelper.GetIpAddress(),
                    Confirmation = true
                };

                dB.COOKIES_CONFIRMATION.Add(cookieConfirm);
                dB.SaveChanges();
            }
        }
    }
}
