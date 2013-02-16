using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Diagnostics;

namespace KBLite.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            string listPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KnowledgebaseFiles\\Content\\content_list.htm");

            if (System.IO.File.Exists(listPath))
            {
                ViewBag.contentHtml = System.IO.File.ReadAllText(listPath);
            }

            return View();
        }
    }
}
