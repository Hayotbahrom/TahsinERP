using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PartStockController : Controller
    {
        // GET: PartStock
        public async Task<ActionResult> Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var userID = GetUserID(User.Identity.Name);
                var list = await db.PART_STOCKS
                                        .Include(x => x.PART_WRHS)
                                        .Include(x => x.PART)
                                        .Where(x => x.PART_WRHS.MRP == userID)            
                                        .ToListAsync();
                return View(list);
            }
        }

        private int? GetUserID(string email)
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                USER currentUser = db.USERS
                                     .Where(u => u.Email.CompareTo(email) == 0 && u.IsDeleted == false && u.IsActive == true)
                                     .FirstOrDefault();

                return currentUser?.ID;
            }
        }
    }
}