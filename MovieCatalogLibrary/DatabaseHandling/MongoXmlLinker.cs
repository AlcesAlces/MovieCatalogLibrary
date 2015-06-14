using MongoDB.Bson;
using MovieCatalogLibrary.MovieClasses;
using SocketIOClient;
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

        static Client socket;

        public async static Task AddMovies(List<Movie> toAdd, string UID, Client socket)
        {
            toAdd.ForEach(x => x.genresCSV = x.getGenreCommaSeperated());
            await AddMoviesDB(toAdd, UID, socket);
            AddMoviesXMLFull(toAdd);
        }

        public async static Task RemoveMovies(List<Movie> toRemove, string UID, Client socket)
        {
            FileHandler handler = new FileHandler();
            handler.removeMovies(toRemove);
            await MongoInteraction.RemoveMovies(toRemove, socket);
        }

        /// <summary>
        /// Add a collection of movies
        /// </summary>
        /// <param name="listOfMoviesXML"></param>
        /// <param name="UID"></param>
        private async static Task AddMoviesDB(List<Movie> listOfMoviesXML, string UID, Client socket)
        {
            await MongoInteraction.AddMovies(listOfMoviesXML, socket);
        }

        /// <summary>
        /// Add a list of CompactMovies (intended to be obtained from the DB).
        /// It should be noted that this function will likely take a long time to execute.
        /// </summary>
        /// <param name="listOfMovies"></param>
        private static void AddMoviesXML(List<Movie> listOfMovies)
        {
            FileHandler handler = new FileHandler();

            handler.addMovies(listOfMovies);
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
        public async static Task SyncUserFiles(string UID, Client socket)
        {
            List<Movie> listOfMoviesDB = await MongoInteraction.AllMoviesByUser(UID, socket);
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
                    await AddMoviesDB(listOfMoviesXML, UID, socket);
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
                        toAdd.Add(new Movie(helper.getTmdbMovieById(item.mid)));
                    }

                    handler.addMovies(toAdd);
                }
            }

            else
            {
                List<Movie> dbBalance = FindDifferences(listOfMoviesDB, listOfMoviesXML);
                if (dbBalance.Count != 0)
                {
                    await AddMoviesDB(dbBalance, UID, socket);
                }

                List<Movie> xmlBalance = FindDifferences(listOfMoviesXML, listOfMoviesDB);
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
        private static List<Movie> FindDifferences(List<Movie> first, List<Movie> second)
        {
            List<Movie> differences = new List<Movie>();


            foreach(var b in second)
            {
                if (first.Where(x => x.mid == b.mid).Count() == 0)
                {
                    differences.Add(b);
                }
            }

            return differences;
        }

    }
}
