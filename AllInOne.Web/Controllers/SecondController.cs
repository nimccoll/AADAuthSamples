using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AllInOne.Web.Controllers
{
    public class SecondController : Controller
    {
        [Authorize]
        public ActionResult Index(int id = 0)
        {
            return View();
        }
    }
}