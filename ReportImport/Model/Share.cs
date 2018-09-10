using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportImport.Model
{
    class Share
    {
        [BsonId]
        public ObjectId  Id { get; set; }
        public string Isin { get; set; }
        public string Ticker { get; set; }
        public string Title { get; set; }
        public int Lot { get; set; }
        public bool IsBond { get; set; }
    }
}
