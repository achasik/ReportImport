using ReportImport.Model;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static ReportImport.Utils;

namespace ReportImport
{
    static class Report
    {
        static XmlDocument doc;
        public static void Load(string fileName)
        {
            doc = new XmlDocument();
            doc.Load(fileName);
        }
        public static List<Position> GetPositions()
        {
            var xpath = "//REPORT_DOC/SECTIONS/DB5/R[@Out!='0.00000']";
            var nodes = doc.DocumentElement.SelectNodes(xpath);
            if (nodes == null) return null;
            return nodes.Cast<XmlNode>().Select(n =>
            new Position
            {
                Ticker = MongoApi.Find<Share>(new { Isin = n.Attributes["ISIN"].InnerText }).Ticker,
                Quantity = ToInt(n.Attributes["Out"].InnerText)
            })
            .ToList();
        }
    }
}
