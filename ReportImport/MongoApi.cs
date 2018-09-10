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

        public static async Task<Share> Get(object query)
        {
            string q = null;
            // if (query != null) q = JsonConvert.SerializeObject(query,new JsonSerializerSettings { NullValueHandling= NullValueHandling.Ignore});
            if (query != null) q = query.ToJson();
            var url = $"{API_URL}/share?apiKey={API_KEY}&fo=true{(q!=null ?"&q="+q :"" )}";
            var json = await Program.Client.GetStringAsync(url);
            // return JsonConvert.DeserializeObject<Share>(json);
            return BsonSerializer.Deserialize<Share>(json);
        }

        public static async Task<Share> Add(Share share)
        {
            // var json = JsonConvert.SerializeObject(share);
            share.Id = ObjectId.GenerateNewId();
            var json = share.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
            var url = $"{API_URL}/share?apiKey={API_KEY}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await Program.Client.PostAsync(url, content);
            var resultContent = await result.Content.ReadAsStringAsync();
            var updated = BsonSerializer.Deserialize<Share>(resultContent);
            // return JsonConvert.DeserializeObject<Share>(resultContent);
            return updated;
        }

    }
}
