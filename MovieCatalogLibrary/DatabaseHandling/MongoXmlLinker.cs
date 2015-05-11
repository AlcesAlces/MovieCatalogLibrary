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
    public static class MongoXmlLinker
    {

        public async static Task AddMovies(List<Movie> toAdd, string UID)
        {
            await AddMoviesDB(toAdd.Select(x => new CompactMovie(x)).ToList(), UID);
            AddMoviesXMLFull(toAdd);
        }

        public async static Task RemoveMovies(List<Movie> toRemove, string UID)
        {
            FileHandler handler = new FileHandler();
            handler.removeMovies(toRemove);

            List<BsonDocument> tempRemoval = new List<BsonDocument>();
            //Remove all movies in the list that are associated with the user.
            tempRemoval.AddRange(toRemove.Select(x=>new BsonDocument().Add("uid",UID).Add("mid",x.mid)).ToList());
            await MongoInteraction.RemoveMovies(tempRemoval);
        }

        /// <summary>
        /// Add a collection of movies
        /// </summary>
        /// <param name="listOfMoviesXML"></param>
        /// <param name="UID"></param>
        private async static Task AddMoviesDB(List<CompactMovie> listOfMoviesXML, string UID)
        {
            List<BsonDocument> toAdd = (from b in listOfMoviesXML
                                        select new BsonDocument().Add("uid", UID).Add(
                                        "mid", b.MID).Add("rate", b.userRating).Add("post", b.posterNum)
                                                ).ToList();

            await MongoInteraction.AddMovies(toAdd);
        }

        /// <summary>
        /// Add a list of CompactMovies (intended to be obtained from the DB).
        /// It should be noted that this function will likely take a long time to execute.
        /// </summary>
        /// <param name="listOfMovies"></param>
        private static void AddMoviesXML(List<CompactMovie> listOfMovies)
        {
            TMDBHelper helper = new TMDBHelper();
            FileHandler handler = new FileHandler();

            List<Movie> toAdd = new List<Movie>();

            foreach (var x in listOfMovies)
            {
                toAdd.Add(new Movie(helper.getTmdbMovieById(x.MID)));
            }

            handler.addMovies(toAdd);
        }

        /// <summary>
        /// Full version of the add movies function.
        /// </summary>
        /// <param name="listOfMovies"></param>
        private static void AddMoviesXMLFull(List<Movie> listOfMovies)
        {
            FileHandler handler = new FileHandler();

            handler.addMovies(listOfMovies);
        }

        /// <summary>
        /// Use to make sure the user files are in line.
        /// </summary>
        /// <param name="UID"></param>
        public async static Task SyncUserFiles(string UID)
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
                    await AddMoviesDB(listOfMoviesXML.Select(x=>new CompactMovie(x)).ToList(), UID);
                }
            }

            //The user has no entries in their XML
            else if (listOfMoviesXML.Count == 0)
            {
                if (listOfMoviesDB.Count == 0)
                {
                    //The user doesn't have any movies in their DB.
                    return;
                }

                else
                {
                    TMDBHelper helper = new TMDBHelper();
                    FileHandler handler = new FileHandler();
                    List<Movie> toAdd = new List<Movie>();

                    foreach (var item in listOfMoviesDB)
                    {
                        toAdd.Add(new Movie(helper.getTmdbMovieById(item.MID)));
                    }

                    handler.addMovies(toAdd);
                }
            }

            else
            {
                List<CompactMovie> dbBalance = FindDifferences(listOfMoviesDB, listOfMoviesXML.Select(x => new CompactMovie(x)).ToList());
                if (dbBalance.Count != 0)
                {
                    await AddMoviesDB(dbBalance, UID);
                }

                List<CompactMovie> xmlBalance = FindDifferences(listOfMoviesXML.Select(x => new CompactMovie(x)).ToList(), listOfMoviesDB);
                if (xmlBalance.Count != 0)
                {
                    AddMoviesXML(xmlBalance);
                }    
            }
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
