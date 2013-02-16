using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KBDocumentConverter.Converters;
using System.Text;

namespace KBLite.Controllers
{
    public class ContentController : Controller
    {
        //
        // GET: /Content/
        [HttpPost]
        [OutputCache(Duration = 120, VaryByParam = "id")]
        public string GetContent(string id)
        {
            // Decrypt string to get filepath.
            string file = EncryptStrings.DecryptAESString(id);

            string content = String.Empty;

            if(System.IO.File.Exists(file))
            {
                content = System.IO.File.ReadAllText(file);
            }

            return content;
        }
    }
}
