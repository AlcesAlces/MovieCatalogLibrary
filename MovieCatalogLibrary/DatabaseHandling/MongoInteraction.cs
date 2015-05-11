using MongoDB.Bson;
using MongoDB.Driver;
using MovieCatalogLibrary.Misc;
using MovieCatalogLibrary.MovieClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.DatabaseHandling
{
    public static class MongoInteraction
    {
        /// <summary>
        /// Creates a mongodb connection and returns the MongoCollection base
        /// </summary>
        /// <param name="collection"></param>
        private static IMongoCollection<BsonDocument> CreateMongoConnection(string table)
        {
            //Configure mongoDB URL.
            MongoUrl url = new MongoUrl(XmlHandler.ReturnDefaultDB());
            MongoClient client = new MongoClient(url);

            //Grab our connection
            IMongoDatabase db = client.GetDatabase(url.DatabaseName);
            //Grab the users collection
            return db.GetCollection<BsonDocument>(table);
        }

        #region User Functions

        /// <summary>
        /// Function to create the user int he database, uses the PasswordHash class to make
        /// a secure password.
        /// </summary>
        /// <param name="user">User name will be made tolower</param>
        /// <param name="pass">Password will be hashed using PasswordHash</param>
        /// <returns>Returns true for success, False for failure.</returns>
        public async static Task<bool> CreateUser(string user, string pass)
        {
            var collection = CreateMongoConnection("users");
            //Ensure that the user doesn't already exist
            var users = await collection.Find(new BsonDocument("user", user)).ToListAsync();
            if(users.Count != 0)
            {
                return false;
            }

            //Create our BSON object to send to the server.
            //Note: You can look into nesting a BsonArray into one of the BsonDocuments, it's cool.
            BsonDocument toAdd = new BsonDocument()
            .Add("user", user)
            .Add("password", PasswordHash.CreateHash(pass)).Add("sync","False");

            await collection.InsertOneAsync(toAdd);
            
            return true;
        }

        /// <summary>
        /// Updates the user based on passed in parameters.
        /// You should only be able to call this if you have a user created by that name.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="sync"></param>
        /// <returns></returns>
        public async static Task UpdateUser(string user, bool sync)
        {
            var collection = CreateMongoConnection("users");
            var users = await collection.Find(new BsonDocument("user", user)).ToListAsync();

            if (users.Count != 0)
            {
                users[0].Set(3, sync.ToString());
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("user", user);
                await collection.ReplaceOneAsync(filter, users[0]);
            }
        }

        public async static Task<bool> GetUserSyncStatus(string user)
        {
            var collection = CreateMongoConnection("users");
            var users = await collection.Find(new BsonDocument("user", user)).ToListAsync();

            if(users.Count != 0)
            {
                if(users[0].Values.ElementAt(3).ToString().ToLower() == "true")
                {
                    return true;
                }
                
                else
                {
                    return false;
                }
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the user's information is correct.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static async Task<bool> VerifyCredentials(string user, string pass)
        {
            var collection = CreateMongoConnection("users");

            //Ensure that the user doesn't already exist
            var users = await collection.Find(new BsonDocument("user", user)).ToListAsync();

            if (PasswordHash.ValidatePassword(pass, users[0].Values.ElementAt(2).ToString()))
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the user ID by their name.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<string> UserIdByName(string user)
        {
            var collection = CreateMongoConnection("users");

            var users = await collection.Find(new BsonDocument("user", user)).ToListAsync();

            //Make sure that the user name exists.
            if(users.Count != 0)
            {
                return users[0].Values.ElementAt(0).ToString();
            }
            //User name didn't exist. Bad things!
            else
            {
                return null;
            }
        }

        #endregion

        #region Movie Functions

        /// <summary>
        /// Find all movies attached to a user ID.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Returned tuple contains: MovieID, UserRating, PosterID</returns>
        public static async Task<List<CompactMovie>> AllMoviesByUser(string userID)
        {
            var collection = CreateMongoConnection("movies");

            var movies = await collection.Find(new BsonDocument("uid", userID)).ToListAsync();

            List<CompactMovie> toReturn = new List<CompactMovie>();

            //Generate a tuple of MID, UserRating, and PosterID for each movie in a user's collection.
            foreach (var item in movies)
            {
                int first = int.Parse(item.Values.ElementAt(2).ToString());
                int second = int.Parse(item.Values.ElementAt(3).ToString());
                int third = int.Parse(item.Values.ElementAt(4).ToString());
                toReturn.Add(new CompactMovie
                            {
                              MID = first,
                              userRating = second,
                              posterNum = third
                            });
            }

            return toReturn;
        }

        /// <summary>
        /// Add a range of movies tot he database.
        /// </summary>
        /// <param name="toAdd"></param>
        public static async Task AddMovies(List<BsonDocument> toAdd)
        {
            var collection = CreateMongoConnection("movies");
            await collection.InsertManyAsync(toAdd);
        }

        /// <summary>
        /// Removes a list of movies from the database.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public static async Task RemoveMovies(List<BsonDocument> toRemove)
        {
            var collection = CreateMongoConnection("movies");
            foreach(var x in toRemove)
            {
                await collection.DeleteOneAsync(x);
            }
            
        }

        #endregion

    }
}
