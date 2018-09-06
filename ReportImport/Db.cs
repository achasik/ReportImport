using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Configuration;

namespace ReportImport
{
    static class Db
    {
        // static readonly MongoClient client;
        static readonly IMongoDatabase db;

        static Db()
        {
            //var connectionString = ConfigurationManager.AppSettings["connectionString"];
            //var client = new MongoClient(connectionString);
            //db = client.GetDatabase(new MongoUrl(connectionString).DatabaseName);

            var mongoClientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress("ds111876.mlab.com", 3128),
                Credential = MongoCredential.CreateCredential("heroku_jslzj311", "heroku_jslzj311", "7agimn65odqrtqcop0dacdr9ri")
            };
            var client = new MongoClient(mongoClientSettings);
            db = client.GetDatabase("heroku_jslzj311");
        }

        public static void Test()
        {
            // var c = db.GetCollection < BsonDocument > ("films");
            Console.WriteLine(CollectionExists("films"));
            Console.WriteLine(CollectionExists("shares"));
        }

        static bool CollectionExists(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var options = new ListCollectionNamesOptions { Filter = filter };

            return db.ListCollectionNames(options).Any();
        }

    }
}
