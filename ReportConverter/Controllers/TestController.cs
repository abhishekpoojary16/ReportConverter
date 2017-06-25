using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReportConverter.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult testMethod()
        {
            return View();
        }

        [HttpGet]
        public string testMethod(string param)
        {
            string message = HttpUtility.HtmlEncode("Parameter = "
+ param);

            return message;
        }

        public ActionResult testM2(int id)
        {
            return View();
        }

    }
}
