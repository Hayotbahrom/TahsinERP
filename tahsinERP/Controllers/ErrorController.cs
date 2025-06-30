using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace tahsinERP.Controllers
{
    [Route("[controller]")]
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            return View();
        }
    }

}