using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportImport.Model
{
    class Trade
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateTime Date { get; set; }
        public string Operation { get; set; }
        public string Ticker { get; set; }
        public ObjectId ShareId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
    }

}
