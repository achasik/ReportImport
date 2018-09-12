using MongoDB.Bson;
using ReportImport.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, fileMask, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                ParseXml(file);
            }
        }

        static void ParseXml(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);
            foreach (XmlNode row in doc.DocumentElement.SelectNodes("//REPORT_DOC/SECTIONS/DB9/R[@ISIN]"))
            {
                var share = GetShare(row);
                if (share == null) continue;
                var trade = GetTrade(row, share);
                var position = GetPosition(share, trade);
            }
        }
        static Position GetPosition(Share share, Trade trade)
        {
            var position = MongoApi.Get<Position>(new { share.Ticker, IsOpen = true });
            if (position == null)
            {
                position = new Position {
                    IsOpen = true,
                    Ticker = share.Ticker,
                    ShareId = share.Id.Value,
                    DateOpen = trade.Date,
                    PriceOpen = trade.Price,
                    Quantity = trade.Quantity,
                    Trades = new List<ObjectId> { trade.Id.Value }
                };
                position = MongoApi.Update<Position>(position);
            }
            if (position.Trades.Any(t => t == trade.Id.Value)) return position;

            if (trade.Operation == "Buy")
            {
                var qty = position.Quantity + trade.Quantity;
                position.PriceOpen = (position.Quantity * position.PriceOpen + trade.Quantity * trade.Price) / qty;
                position.Quantity = qty;
                position.Trades.Add(trade.Id.Value);
            }
            else
            {
                var a = 1;
            }
            position = MongoApi.Update<Position>(position);
            return position;
        }
        static Trade GetTrade(XmlNode row, Share share)
        {
            var trade = new Trade
            {
                Date = Convert.ToDateTime(row.Attributes["D"].Value + " " + row.Attributes["T"].Value),
                Operation = row.Attributes["Op"].Value == "Покупка" ? "Buy" : "Sell",
                Ticker = share.Ticker,
                ShareId = share.Id.Value,
                Quantity = ToInt(row.Attributes["Qty"].Value),
                Price = share.IsBond ?
                        ToDouble(row.Attributes["Pr"].Value) + ToDouble(row.Attributes["ACY"].Value) / ToInt(row.Attributes["Qty"].Value) :
                        ToDouble(row.Attributes["Pr"].Value),
                Amount = share.IsBond ? ToDouble(row.Attributes["SPrA"].Value) : ToDouble(row.Attributes["SPr"].Value)
            };

            var existed = MongoApi.Get<Trade>(new { trade.Date, trade.Ticker, trade.Operation, trade.Quantity });
            if (existed != null) return existed;
            // trade.Id = existed.Id;
            var updated = MongoApi.Update<Trade>(trade);
            return updated;
        }
        static Share GetShare(XmlNode row)
        {
            var Isin = row.Attributes["ISIN"].Value;
            var isBond = row.Attributes["ACY"] != null;

            var share = MongoApi.Get<Share>(new { Isin });
            if (share == null)
                share = isBond ? AllShares.GetShare(Isin) : AllShares.FinBond(Isin);
            if (share == null)
            {
                Logger.Log($"ISIN {Isin} not found");
                return null;
            }
            if (!share.Id.HasValue)
            {
                share = MongoApi.Update<Share>(share);
            }
            return share;
        }
        static int ToInt(string s)
        {
            int i;
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
                return i;
            throw new ArgumentException($"ToInt failed {s}");
        }
        static double ToDouble(string s)
        {
            double d;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return d;
            throw new ArgumentException($"ToDouble failed {s}");
        }


    }
}
