using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Homework3
{
    class Student
    {
        public double _id { get; set; }
        public string name { get; set; }
        public List<Score> scores { get; set; }
        public BsonDocument ExtraElements { get; set; }
    }
    class Score
    {
        public string type { get; set; }
        public double score { get; set; }
    }

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
            BsonClassMap.RegisterClassMap<Student>();
            var client = new MongoClient();
            var db = client.GetDatabase("school");
            var collection = db.GetCollection<Student>("students");

            // Iterate through all students one at a time
            var students = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var student in students)
            {
                // Identify the lowest homework score for this student
                double lowestScore = -1.0;
                foreach (var score in student.scores)
                {
                    if (score.type == "homework")
                    {
                        if (lowestScore == -1 || score.score < lowestScore) 
                            lowestScore = score.score;
                    }
                }

                // Remove that score
                foreach (var score in student.scores)
                {
                    if (score.type == "homework" && score.score == lowestScore)
                    {
                        student.scores.Remove(score);
                        Console.WriteLine("Removing score {0} for student {1}", score.score, student.name);
                        break;
                    }
                }

                // Update the student in the DB
                var result = await collection.ReplaceOneAsync(
                    Builders<Student>.Filter.Eq("_id", student._id),
                    student
                );
            }
        }
    }
}
