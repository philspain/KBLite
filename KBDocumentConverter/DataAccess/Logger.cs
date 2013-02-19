using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace KBDocumentConverter.DataAccess
{
    public static class Logger
    {
        // Path for directory that will contain logs
        static readonly string dirName = Path.Combine(
            Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)), "logs");

        // Path for file to log errors
        static readonly string errorFile = Path.Combine(dirName, "error-log.txt");

        /// <summary>
        /// Attempt to append error message to log file.
        /// </summary>
        /// <param name="error">Error message to be added.</param>
        public static void LogError(string error)
        {
            CheckDirectoryExists();
            CheckFileExists();

            using (FileStream fs = File.Open(errorFile, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string message = "\n------ Begin Error ------" +
                        "\n" + error + "\n" +
                        "------ End Error ------\n";

                    sw.Write(message);
                }
            }
        }

        /// <summary>
        /// Check if directory for logs existts.
        /// </summary>
        static private void CheckDirectoryExists()
        {
            if (!Directory.Exists(dirName))
            {
                try
                {
                    Directory.CreateDirectory(dirName);
                }
                catch { }
            }
        }

        /// <summary>
        /// Check if log file exists, create if it doesn't
        /// </summary>
        static void CheckFileExists()
        {
            if (!File.Exists(errorFile))
            {
                    FileStream fs = File.Create(errorFile);
                    fs.Close();
            }
        }
    }
}
