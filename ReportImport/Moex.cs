using MongoDB.Bson;
using ReportImport.Model;
using System;
using System.Xml;

namespace ReportImport
{
    static class Moex
    {
        static XmlDocument doc;
        static Moex()
        {
            doc = new XmlDocument();
            doc.Load("https://iss.moex.com/iss/engines/stock/markets/shares/securities.xml");
        }
        
        public static Share GetShare(string isin)
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode($"//data//rows/row[(@BOARDID='TQBR' or @BOARDID='TQTF') and @ISIN='{isin}']");
            if (node == null) return null;
            var share = new Share
                {   Isin = isin,
                    Ticker = node.Attributes["SECID"].Value,
                    Title = node.Attributes["SHORTNAME"].Value,
                    Lot = Convert.ToInt32(node.Attributes["LOTSIZE"].Value),
                    IsBond=false
                };
            if (share == null)
            {
                if (isin == "RU000A0JR5Z5")
                {
                    share = new Share { Isin = "RU000A0JR5Z5", Ticker = "RUALR", Title = "Русал РДР", Lot = 10, IsBond = false };
                }
            }
            return share;
        }

        public static Share GetBond(string isin)
        {
            var url = $"https://iss.moex.com/iss/securities.xml?q={isin}&iss.meta=off";
            var doc = new XmlDocument();
            doc.Load(url);
            XmlNode node = doc.DocumentElement.SelectSingleNode($"//data//rows/row[@isin='{isin}']");
            if (node == null) return null;
            var share = new Share
            {
                Isin = isin,
                Ticker = node.Attributes["secid"].Value,
                Title = node.Attributes["shortname"].Value,
                Lot = 1,
                IsBond = true
            };
            return share;
        }
    }
}
