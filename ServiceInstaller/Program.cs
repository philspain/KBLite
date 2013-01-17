using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration.Install;
using System.IO;

namespace ServiceInstaller
{
    class Program
    {
        private static readonly string _servicePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\KBDocumentConverterService.exe";

        static void Main(string[] args)
        {         
            try
            {
                ManagedInstallerClass.InstallHelper( new string[]{ _servicePath } );
            }
            catch
            {

            }

        }
    }
}
