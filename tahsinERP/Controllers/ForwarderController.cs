using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class ForwarderController : Controller
    {
        // GET: Forwarder
        private  DBTHSNEntities db { get; }
        public ForwarderController(DBTHSNEntities db)
        {
            this.db = db;
        }

        public ActionResult Index()
        {
            /*using (DBTHSNEntities db1 = new DBTHSNEntities())
            {*/
                var list = db.FORWARDERS.Where(f => f.IsDeleted == false ).ToList();
                return View(list);
            
        }
    }
}