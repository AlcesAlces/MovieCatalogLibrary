using MongoDB.Bson;
using MovieCatalogLibrary.Misc;
using MovieCatalogLibrary.MovieClasses;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.DatabaseHandling
{
    public static class MongoInteraction
    {

        #region User Functions

        /// <summary>
        /// Function to create the user int he database, uses the PasswordHash class to make
        /// a secure password.
        /// </summary>
        /// <param name="user">User name will be made tolower</param>
        /// <param name="pass">Password will be hashed using PasswordHash</param>
        /// <returns>Returns true for success, False for failure.</returns>
        public async static Task<bool> CreateUser(string user, string pass, Client socket)
        {
            if (socket.IsConnected)
            {
                //Basically this is fine, as long as encryption is added.
                socket.Emit("create", new { user = user.ToLower(), password = pass.ToLower(), sync = false });
            }
            
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
        public async static Task UpdateUser(string user, bool sync, Client socket)
        {

            if(socket.IsConnected)
            {
                //TODO: Fix this
                socket.Emit("update_user", new { what = sync });
            }

        }

        public async static Task<bool> GetUserSyncStatus(string user)
        {
            //TODO: Add code :V
            return false;
        }

        /// <summary>
        /// Returns true if the user's information is correct.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static async Task<bool> VerifyCredentials(string user, string pass, Client socket)
        {

            bool? auth = null;

            socket.On("authenticate", (fn) =>
                {
                    //Something
                    if (fn.Json.Args[0].ToString() == "authenitcated")
                    {
                        auth = true;
                    }
                    else
                    {
                        auth = false;
                    }
                });

            if (socket.IsConnected)
            {
                socket.Emit("authenticate", new { user = user, password = pass });
            }

            while (auth == null) ;

            return (bool)auth;
        }

        /// <summary>
        /// Retrieve the user ID by their name.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        //public static async Task<string> UserIdByName(string user)
        //{
        //    var collection = CreateMongoConnection("users");

        //    var users = await collection.Find(new BsonDocument("user", user)).ToListAsync();

        //    //Make sure that the user name exists.
        //    if(users.Count != 0)
        //    {
        //        return users[0].Values.ElementAt(0).ToString();
        //    }
        //    //User name didn't exist. Bad things!
        //    else
        //    {
        //        return null;
        //    }
        //}

        #endregion

        #region Movie Functions

        /// <summary>
        /// Find all movies attached to a user ID.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Returned tuple contains: MovieID, UserRating, PosterID</returns>
        public static async Task<List<Movie>> AllMoviesByUser(string userID, Client socket)
        {

            List<Movie> toReturn = null;

            socket.On("movie_collection_result", (fn) =>
                {
                    toReturn = MiscHelpers.Equals(fn.Json.Args[0]);
                });

            while (toReturn == null) ;
            
            return toReturn;
        }

        /// <summary>
        /// Add a range of movies tot he database.
        /// </summary>
        /// <param name="toAdd"></param>
        public static async Task AddMovies(List<Movie> toAdd, Client socket)
        {   
            if(socket.IsConnected)
            {
                socket.Emit("add_movie_collection", toAdd);
            }
        }

        /// <summary>
        /// Removes a list of movies from the database.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public static async Task RemoveMovies(List<Movie> toRemove, Client socket)
        {
            socket.Emit("remove_movie_collection", toRemove);
            
        }

        #endregion

    }
}
