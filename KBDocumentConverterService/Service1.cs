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
    using KBDocumentConverterService.Converters;

    namespace KBDocumentConverterService
    {
        public partial class Service1 : ServiceBase
        {
            Thread conversionThread;

            public Service1()
            {
                InitializeComponent();
                this.ServiceName = "KBDocumentConverterService";
            }

            public string Name
            {
                get { return this.ServiceName; }
            }

            protected void StartConversion()
            {
                try
                {
                    while (true)
                    {
                        if(!File.Exists("C:\\Service.txt")) 
                            File.Create("C:\\Service.txt").Close();

                        ConvertToHtml.RunConversion();
                        Thread.Sleep(60000);
                    }
                }
                catch (Exception ex)
                {
                    StreamWriter sw = File.AppendText("C:\\Service.txt");
                    string mess = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                    sw.WriteLine(mess);
                    sw.Flush(); 
                    sw.Close();
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
