using System.Xml;

namespace ReportImport
{
    static class AllShares
    {
        static XmlDocument doc;
        static AllShares()
        {
            doc = new XmlDocument();
            doc.Load("https://iss.moex.com/iss/engines/stock/markets/shares/securities.xml");
        }
        
        public static void test()
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode("//data//rows/row[@BOARDID='TQBR' and @ISIN='RU0007661625']");
        }
    }
}
