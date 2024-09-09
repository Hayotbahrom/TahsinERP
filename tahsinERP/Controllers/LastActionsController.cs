using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class LastActionsController : Controller
    {
        // GET: LastActions
        public async Task<ActionResult> Index(int userId = 1,
                                              DateTime? startDate = null,
                                              DateTime? endDate = null)
        {
            // Agar startDate yoki endDate null bo'lsa, default qiymatlarni beramiz
            startDate = startDate ?? DateTime.Now.AddDays(-30); // So'nggi 30 kunlik ma'lumot
            endDate = endDate ?? DateTime.Now; // Bugungi sana

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                // User loglarini berilgan userId va sanalar oralig'ida filterlash
                List<USERLOG> logs = await db.USERLOGS
                    .Where(x => x.UserID == userId
                                && x.DateTime >= startDate
                                && x.DateTime <= endDate)
                    .ToListAsync();

                // Userlarni DropDownList uchun tayyorlash
                ViewBag.Users = new SelectList(await db.USERS.ToListAsync(), "ID", "Uname", userId);

                // Foydalanuvchiga form orqali jo'natiladigan start va end sanalar
                ViewBag.StartDate = startDate.Value.ToString("dd-MM-yyyy");
                ViewBag.EndDate = endDate.Value.ToString("dd-MM-yyyy");

                return View(logs);
            }
        }
    }
}
