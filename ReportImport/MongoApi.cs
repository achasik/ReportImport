using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using ReportImport.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ReportImport
{
    static class MongoApi
    {
        static readonly string API_KEY;
        static readonly string API_URL;

        static MongoApi()
        {
            API_KEY = ConfigurationManager.AppSettings["mongoApiKey"];
            var dbase = ConfigurationManager.AppSettings["mongoDbase"];
            API_URL = "https://api.mlab.com/api/1/databases/"+dbase+"/collections";
        }

        public static async Task<Share> GetShare(object query)
        {
            string q = null;
            if (query != null) q = query.ToJson();
            var url = $"{API_URL}/share?apiKey={API_KEY}&fo=true{(q!=null ?"&q="+q :"" )}";
            var json = await Program.Client.GetStringAsync(url);
            return BsonSerializer.Deserialize<Share>(json);
        }

        public static async Task<Share> AddShare(Share share)
        {
            share.Id = ObjectId.GenerateNewId();
            var json = share.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
            var url = $"{API_URL}/share?apiKey={API_KEY}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await Program.Client.PostAsync(url, content);
            var resultContent = await result.Content.ReadAsStringAsync();
            var updated = BsonSerializer.Deserialize<Share>(resultContent);
            return updated;
        }

    }
}
