using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Homework2
{
    class Program
    {
        static void Main()
        {
            MainAsync().Wait();
            Console.WriteLine("Press Enter");
            Console.ReadLine();
        }

        static async Task MainAsync()
        {
            var client = new MongoClient();
            var db = client.GetDatabase("students");
            var collection = db.GetCollection<BsonDocument>("grades");

            // Iterate through all students one at a time
            var documents = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var studentId in documents.GroupBy(doc => doc["student_id"]))
            {
                // Iterate through all of the "homework" scores for this student
                var filterBuilder = Builders<BsonDocument>.Filter;
                var homeworkFilter = filterBuilder.Eq("student_id", studentId.Key) & filterBuilder.Eq("type", "homework");
                var homeworkScores = await collection.Find(homeworkFilter).ToListAsync();

                // Identify the lowest score
                BsonDocument lowestScore = null;
                foreach (var score in homeworkScores)
                {
                    if (lowestScore == null || score["score"] < lowestScore["score"])
                    {
                        lowestScore = score;
                    }
                }

                // Remove that score from the collection
                if (lowestScore != null)
                {
                    await collection.DeleteOneAsync(doc => doc["_id"] == lowestScore["_id"]);
                }
            }
        }
    }
}
