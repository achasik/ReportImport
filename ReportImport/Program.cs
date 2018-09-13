using MongoDB.Bson;
using ReportImport.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using static ReportImport.Utils;

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
                Report.Load(file);
                ParseXml(file);
                CheckPosition();
            }
        }

        private static void CheckPosition()
        {
            var fromDb = MongoApi.Get<Position[]>(new { IsOpen = true });
            var fromReport = Report.GetPositions();
            if (fromDb.Length != fromReport.Count) Log($"Position Length mismatch db:{fromDb.Length} report: {fromReport.Count}");
            foreach (var pos in fromDb)
            {
                var pos2 = fromReport.FirstOrDefault(p => p.Ticker == pos.Ticker);
                if (pos2 == null)
                {
                    Log($"Ticker {pos.Ticker} not found in report");
                    continue;
                }
                if (pos.Quantity != pos2.Quantity) Log($"Ticker {pos.Ticker} quantity mismatch {pos.Quantity} {pos2.Quantity}");
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
            foreach (XmlNode row in doc.DocumentElement.SelectNodes("//REPORT_DOC/SECTIONS/DB8/R"))
            {
                var Isin = row.Attributes["ISIN"].InnerText;
                var share = MongoApi.Find<Share>(new { Isin });
                if (share == null)
                {
                    Log($"Isin not found {Isin}");
                    continue;
                }
                var op = row.Attributes["Op"].InnerText;
                if (op != "Глобальная операция (списание)")
                {
                    Log($"Opertaion not found {op}");
                    continue;
                }
                var trade = new Trade
                {
                    Operation = "Sell",
                    Date = DateTime.Parse(row.Attributes["D"].InnerText),
                    Ticker = share.Ticker,
                    Price = 1000,
                    Quantity = ToInt(row.Attributes["Dec"].InnerText),
                    Amount = 1000 * ToInt(row.Attributes["Dec"].InnerText),
                };
                var existed = MongoApi.Find<Trade>(new { trade.Date, trade.Ticker, trade.Operation, trade.Quantity });
                trade = existed ?? MongoApi.Update<Trade>(trade);
                GetPosition(share, trade);
            }
        }
        static Position GetPosition(Share share, Trade trade)
        {
            var tradeExist = MongoApi.Find<Position>(new { Trades = trade.Id });
            if (tradeExist != null) return null;
            var position = MongoApi.Find<Position>(new { share.Ticker, IsOpen = true });
            if (position == null)
            {
                position = new Position
                {
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
            }
            else
            {
                var qty = position.Quantity - trade.Quantity;
                if (qty < 0)
                {
                    Log($"Position is negative {position.Ticker} {position.Quantity}");
                    return position;
                }
                position.Quantity = qty;
                if (position.Quantity == 0)
                {
                    position.IsOpen = false;
                    position.PriceClose = trade.Price;
                    position.DateClose = trade.Date;
                }
                else
                {
                    var closed = new Position
                    {
                        IsOpen = false,
                        DateOpen = position.DateOpen,
                        DateClose = trade.Date,
                        PriceOpen = position.PriceOpen,
                        PriceClose = trade.Price,
                        Quantity = trade.Quantity,
                        ShareId = trade.ShareId,
                        Ticker = trade.Ticker
                    };
                    MongoApi.Update<Position>(closed);
                }
            }
            position.Trades.Add(trade.Id.Value);
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

            var existed = MongoApi.Find<Trade>(new { trade.Date, trade.Ticker, trade.Operation, trade.Quantity });
            if (existed != null) return existed;
            // trade.Id = existed.Id;
            var updated = MongoApi.Update<Trade>(trade);
            return updated;
        }
        static Share GetShare(XmlNode row)
        {
            var Isin = row.Attributes["ISIN"].Value;
            var isBond = row.Attributes["ACY"] != null;

            var share = MongoApi.Find<Share>(new { Isin });
            if (share == null)
                share = isBond ? Moex.GetShare(Isin) : Moex.GetBond(Isin);
            if (share == null)
            {
                Log($"ISIN {Isin} not found");
                return null;
            }
            if (!share.Id.HasValue)
            {
                share = MongoApi.Update<Share>(share);
            }
            return share;
        }



    }
}
