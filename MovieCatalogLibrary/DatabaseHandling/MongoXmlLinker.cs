using MongoDB.Bson;
using MovieCatalogLibrary.MovieClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.DatabaseHandling
{
    /// <summary>
    /// The purpose of this class is to make the XML and the mongoDB play nice.
    /// </summary>
    static class MongoXmlLinker
    {

        public static void AddMovie()
        {

        }

        public static void RemoveMovie()
        {

        }

        /// <summary>
        /// Add a collection of movies
        /// </summary>
        /// <param name="listOfMoviesXML"></param>
        /// <param name="UID"></param>
        private async static void AddMoviesDB(List<Movie> listOfMoviesXML, string UID)
        {
            List<BsonDocument> toAdd = (from b in listOfMoviesXML
                                        select new BsonDocument().Add("uid", UID).Add(
                                        "mid", b.mid).Add("rate", b.userRating).Add("post", b.poster)
                                                ).ToList();

            await MongoInteraction.AddMovies(toAdd);
        }

        /// <summary>
        /// Use to make sure the user files are in line.
        /// </summary>
        /// <param name="UID"></param>
        public async static void SyncUserFiles(string UID)
        {
            List<CompactMovie> listOfMoviesDB = await MongoInteraction.AllMoviesByUser(UID);
            FileHandler tmpFileHandler = new FileHandler();
            List<Movie> listOfMoviesXML = tmpFileHandler.allMoviesInXml();

            //The user has no entries on the DB
            if(listOfMoviesDB.Count == 0)
            {
                
                if(listOfMoviesXML.Count == 0)
                {
                    //The user doesn't have any movies in their XML either.
                    return;
                }

                else
                {
                    AddMoviesDB(listOfMoviesXML, UID);
                }
            }

            //The user has no entries in their XML
            if(listOfMoviesXML.Count == 0)
            {
                if(listOfMoviesDB.Count == 0)
                {
                    //The user doesn't have any movies in their DB.
                    return;
                }

                else
                {
                    TMDBHelper helper = new TMDBHelper();
                    FileHandler handler = new FileHandler();
                    foreach(var item in listOfMoviesDB)
                    {
                        Movie toAdd = new Movie(helper.getTmdbMovieById(item.MID));
                        handler.addMovie(toAdd);
                    }
                }
            }

            //TODO: Balance the DB and the XML
        }

        /// <summary>
        /// Find the differences between two collections of CompactMovies.
        /// Will find the entries that exist in second, that do not exist in first.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static List<CompactMovie> FindDifferences(List<CompactMovie> first, List<CompactMovie> second)
        {
            List<CompactMovie> differences = new List<CompactMovie>();


            foreach(var b in second)
            {
                if (first.Where(x => x.MID == b.MID).Count() == 0)
                {
                    differences.Add(b);
                }
            }

            return differences;
        }

    }
}
