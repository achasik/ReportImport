using System;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ReportImport
{
    static class Db
    {
        // static readonly MongoClient client;
        static readonly IMongoDatabase db;

        static Db()
        {
            var connectionString = ConfigurationManager.AppSettings["connectionString"];
            var client = new MongoClient(connectionString);
            db = client.GetDatabase(new MongoUrl(connectionString).DatabaseName);
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
