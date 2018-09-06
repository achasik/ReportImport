using System;
using System.IO;
using System.Net.Http;
using System.Xml;

namespace ReportImport
{
    class Program
    {
        public static HttpClient Client = new HttpClient();
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage ReportImport.exe filemask");
                return;
            }
            var fileMask = args[0];
            // AllShares.test();
            Db.Test();
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, fileMask, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                // ParseXml(file);
            }
        }

        static void ParseXml(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);
            foreach (XmlNode row in doc.DocumentElement.SelectNodes("//REPORT_DOC/SECTIONS/DB9/R[@ISIN]"))
            {
                // var ticker = GetTicker(row.Attributes["ISIN"].Value);
                // if (string.IsNullOrEmpty(ticker)) continue;
                //var date = row.Attributes["D"].Value;
                //var operation = row.Attributes["Op"].Value == "Покупка" ? "Buy" : "Sell";
                //var quantity = row.Attributes["Qty"].Value.Substring(0, row.Attributes["Qty"].Value.IndexOf('.'));
                //var price = row.Attributes["Pr"].Value.Replace(".", ",");
                //var nkd = "";
                //if (row.Attributes["ACY"] != null)
                //{
                //    nkd = (Convert.ToDouble(row.Attributes["ACY"].Value.Replace(".", ",")) / Convert.ToDouble(row.Attributes["Qty"].Value.Substring(0, row.Attributes["Qty"].Value.IndexOf('.')))).ToString();
                //}
            }

        }
    }
}
