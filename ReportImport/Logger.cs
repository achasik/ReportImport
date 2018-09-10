using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportImport
{
    public static class Logger
    {
        static string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmm")}-log.txt";
        public static void Log(string msg)
        {
            File.AppendAllText(fileName, $"{DateTime.Now.ToString()} : {msg}{Environment.NewLine}");
            Console.WriteLine(msg);
        }
    }
}
