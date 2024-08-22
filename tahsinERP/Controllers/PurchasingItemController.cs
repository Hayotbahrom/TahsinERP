using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class PurchasingItemController : Controller
    {
        public ActionResult Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                return View();
            }
        }
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Edit(int? id)
        {
            return View();
        }
        public ActionResult Delete(int? id)
        {
            return View();
        }
        public ActionResult Details(int? id)
        {
            return View();
        }

    }

}