using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportImport.Model
{
    class Position
    {
        [BsonId]
        public ObjectId? Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateOpen { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DateClose { get; set; }

        public bool IsOpen { get; set; }

        public string Ticker { get; set; }

        public ObjectId ShareId { get; set; }

        public int Quantity { get; set; }
        public double PriceOpen { get; set; }
        public double? PriceClose { get; set; }

        public List<ObjectId> Trades { get; set; }

    }
}
