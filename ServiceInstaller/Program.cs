using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;
using KBDocumentConverter.DataAccess;

namespace ServiceInstaller
{
    class Program
    {
        // Path for service to install
        static readonly string _servicePath = "KBDocumentConverter.exe";

        static readonly long _timeoutMillisecs = 1000;

        static void Main(string[] args)
        {         
            try
            {
                ManagedInstallerClass.InstallHelper( new string[]{ _servicePath } );
            }
            catch (Exception ex)
            {
                string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                Logger.LogError(message);
            }
        }
    }
}