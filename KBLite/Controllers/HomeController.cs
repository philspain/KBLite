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
            string listPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KnowledgebaseFiles/content/content_list.htm");
            Console.WriteLine(listPath);
            string newListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KnowledgebaseFiles/content/content_list_new.htm");
            Console.WriteLine(newListPath);

            if (System.IO.File.Exists(newListPath))
            {
                if (System.IO.File.Exists(listPath))
                {
                    System.IO.File.Delete(listPath);
                    System.IO.File.Move(newListPath, listPath);
                    ViewBag.contentHtml = System.IO.File.ReadAllText(listPath);
                }
                else
                {
                    ViewBag.contentHtml = "";
                }
            }
            else if (System.IO.File.Exists(listPath))
            {
                ViewBag.contentHtml = System.IO.File.ReadAllText(listPath);
            }
            else
            {
                ViewBag.contentHtml = "";
            }

            return View();
        }
    }
}
