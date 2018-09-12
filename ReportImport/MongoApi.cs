﻿using MongoDB.Bson;
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

        public static T Get<T>(object query)
        {
            string q = null;
            if (query != null) q = query.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
            var collection = CollectionName<T>();
            var url = $"{API_URL}/{collection}?apiKey={API_KEY}&fo=true{(q != null ? "&q=" + q : "")}";
            var json = Program.Client.GetStringAsync(url).Result;
            return BsonSerializer.Deserialize<T>(json);
        }


        public static T Update<T>(T obj)
        {
            // share.Id = ObjectId.GenerateNewId();
            var collection = CollectionName<T>();
            var json = obj.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
            var url = $"{API_URL}/{collection}?apiKey={API_KEY}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = Program.Client.PostAsync(url, content).Result;
            var resultContent = result.Content.ReadAsStringAsync().Result;
            var updated = BsonSerializer.Deserialize<T>(resultContent);
            return updated;
        }

        static string CollectionName<T>()
        {
            var name = typeof(T).Name;
            if (name == "Share") return "share";
            if (name == "Trade") return "trade";
            if (name == "Position") return "position";
            throw new ArgumentException($"Unknown typename {name}");
                
        }        

    }
}
