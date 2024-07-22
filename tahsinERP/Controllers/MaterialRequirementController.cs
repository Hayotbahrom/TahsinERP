using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.Controllers
{
    public class MaterialRequirementController : Controller
    {
        // GET: MaterialRequirement
        public async Task<ActionResult> Index()
        {
            using (DBTHSNEntities db = new DBTHSNEntities())
            {
                //var list = await db.MaterialRequirement.ToListAsync();
                MaterialRequirement materialRequirement = new MaterialRequirement();
                
                return View();
            }
        }
    }
}