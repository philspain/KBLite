using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Threading;
using KBDocumentConverter.Converters;
using KBDocumentConverter.DataAccess;

    namespace KBDocumentConverter
    {
        public partial class Service1 : ServiceBase
        {
            Thread conversionThread;

            public Service1()
            {
                InitializeComponent();
                this.ServiceName = "KBDocumentConverter";
            }

            public string Name
            {
                get { return this.ServiceName; }
            }

            protected void StartConversion()
            {
                while (true)
                {
                    try
                    {
                        ConvertToHtml.RunConversion();
                        Thread.Sleep(60000);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                        Logger.LogError(message);
                    }
                }
            }

            protected override void OnStart(string[] args)
            {
                conversionThread = new Thread(new ThreadStart(StartConversion));

                conversionThread.Start();
            }

            protected override void OnStop()
            {
            }
        }
    }
