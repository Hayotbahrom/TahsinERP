using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PartWRHSController : Controller
    {
        // GET: PartWRHS
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                var part_wrhs = db.PART_WRHS.Where(x => x.IsDeleted == false).ToList();
                return View(part_wrhs);
            }
        }

        public ActionResult Create()
        {
            using(DBTHSNEntities db = new DBTHSNEntities())
            {

            ViewBag.ShopList = new SelectList(db.PROD_SHOPS.ToList(), "ID", "ShopName");

            //var mrpUsers = db.USERS.Where(u => u. == "MRP").ToList();
            ////ViewBag.MRPUsers = new SelectList(mrpUsers, "ID", "FullName");

            return View();
            }
        }

        // POST: PartWhs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,WHName,Description,IsDeleted,MRP,ShopID")] PART_WRHS partWrhs)
        {
            List<PROD_SHOPS> shops;
            List<USER> mrpUsers;

            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                if (ModelState.IsValid)
                {
                    db.PART_WRHS.Add(partWrhs);
                    db.SaveChanges();
                    return RedirectToAction("Index"); // Assuming you have an Index action to list PART_WRHS records
                }

                // Fetching shops
                shops = db.PROD_SHOPS.ToList();

                // Fetching users with the "MRP" role using a join
                //mrpUsers = (from u in db.USERS
                //            join url in db.USER_ROLE_LINK on u.ID equals url.UserID
                //            join r in db.ROLES on url.RoleID equals r.ID
                //            where r.RoleName == "MRP"
                //            select u).ToList();
            }

            ViewBag.ShopID = new SelectList(shops, "ID", "Name", partWrhs.ShopID);
            //ViewBag.MRP = new SelectList(mrpUsers, "ID", "UserName", partWrhs.MRP);

            return View(partWrhs);
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult Delete()
        {
            return View();

        }

        public ActionResult Details()
        {
            return View();
        }
    }
}