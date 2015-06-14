using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MovieCatalogLibrary
{

    /// <summary>
    /// Contains all functions which deal with the saving and loading of user files
    /// </summary>
    public class FileHandler
    {

        log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// Use to determine which platform we're on, it should be noted that
        /// the "Android" platform here is for Xamarin Android.
        /// </summary>
        public enum platformType {Windows, Android};

        private platformType platform { get; set; }

        /// <summary>
        /// Need to use an instance of the FileHandler object for reasons such as:
        /// Xamarin didn't like the static methods.
        /// </summary>
        /// <param name="type"></param>
        public FileHandler(platformType type = platformType.Windows)
        {
            platform = type;
        }

        public FileHandler()
        {
            platform = platformType.Windows;
            LoadLoggerConfig();
        }

        private static void LoadLoggerConfig()
        {
            FileInfo file = new FileInfo("MovieCatalogLibrary.dll.config");

            if(file.Exists)
            {
                log4net.Config.XmlConfigurator.Configure(file);
            }

        }

        /// <summary>
        /// Ensure that the user file exists.
        /// </summary>
        /// <returns></returns>
        public bool checkUserFileExists()
        {
            return File.Exists(getUserPath());
        }

        /// <summary>
        /// Returns the relative user path, the path will be different if you are using Xamarin.
        /// </summary>
        /// <returns>Returns Null if there's an error.</returns>
        public string getUserPath()
        {
            if (platform == platformType.Windows)
            {
                string path = "userinfo.xml";
                return Path.Combine(Directory.GetCurrentDirectory(), path);
            }
            else if (platform == platformType.Android)
            {
                string saveLoc = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                saveLoc += @"/userinfo.xml";
                return saveLoc;
            }

            else
            {
                return null;
            }
        }

        //checks the integrity of the user file.
        public void verifyUserFile()
        {

            log.Debug("Checking to see if user file exists...");

            //User file does exist
            if (checkUserFileExists())
            {
                //Verify that the contents of the user's file follow the guidelines.
                log.Debug("User file does exist, that's a good thing");
            }

            else
            {
                log.Debug("User file doesn't exist, I should be creating the user file, now!");
                //Creates a blank template for the user's file.
                createUserFile();
            }
        }

        /// <summary>
        /// Creates a default user file.
        /// </summary>
        public void createUserFile()
        {
            XmlTextWriter writer = new XmlTextWriter("userinfo.xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("usersettings");
            //Where all the movie data is going to be stored.
            writer.WriteStartElement("movies");
            writer.WriteEndElement();
            //where all of the user settings are going to be stored.
            writer.WriteStartElement("settings");
            writer.WriteEndElement();
            //write end of usersettings
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        /// <summary>
        /// Add a single 'Movie' object to the XML file.
        /// </summary>
        /// <param name="movie">Pass in a fully populated movie object</param>
        public void addMovies(List<Movie> movieList)
        {
            XmlDocument xml = new XmlDocument();

            if(!File.Exists(getUserPath()))
            {
                createUserFile();
            }

            xml.Load(getUserPath());

            XmlNode usersettings = xml.SelectSingleNode("usersettings");
            XmlNode movies = usersettings.SelectSingleNode("movies");

            foreach (Movie movie in movieList)
            {
                //New movie node
                XmlNode newSub = xml.CreateNode(XmlNodeType.Element, "movie", null);

                List<XmlNode> subList = new List<XmlNode>();

                XmlNode nameSub = xml.CreateNode(XmlNodeType.Element, "name", null);
                nameSub.InnerText = movie.name.ToString();
                subList.Add(nameSub);
                XmlNode genSub = xml.CreateNode(XmlNodeType.Element, "genres", null);
                genSub.InnerText = movie.getGenreCommaSeperated();
                subList.Add(genSub);
                XmlNode yearSub = xml.CreateNode(XmlNodeType.Element, "year", null);
                yearSub.InnerText = movie.year.ToString();
                subList.Add(yearSub);
                XmlNode imageSub = xml.CreateNode(XmlNodeType.Element, "image", null);
                imageSub.InnerText = movie.imageLocation;
                subList.Add(imageSub);
                XmlNode movieidSub = xml.CreateNode(XmlNodeType.Element, "movieid", null);
                movieidSub.InnerText = movie.mid.ToString();
                subList.Add(movieidSub);
                XmlNode ratingSub = xml.CreateNode(XmlNodeType.Element, "rating", null);
                ratingSub.InnerText = movie.onlineRating.ToString();
                subList.Add(ratingSub);
                XmlNode descSub = xml.CreateNode(XmlNodeType.Element, "description", null);
                descSub.InnerText = movie.description;
                subList.Add(descSub);
                XmlNode posterSub = xml.CreateNode(XmlNodeType.Element, "posternum", null);
                posterSub.InnerText = "0";
                subList.Add(posterSub);
                XmlNode userratingSub = xml.CreateNode(XmlNodeType.Element, "userrating", null);
                try
                {
                    userratingSub.InnerText = movie.userRating.ToString();
                }
                catch (Exception e)
                {
                    userratingSub.InnerText = "0";
                }

                subList.Add(userratingSub);

                foreach (XmlNode node in subList)
                {
                    newSub.AppendChild(node);
                }

                movies.AppendChild(newSub);
            }

            xml.Save(getUserPath());
        }


        //Get all of the movies in the user's XML file.
        public List<Movie> allMoviesInXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(getUserPath());

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/usersettings/movies/movie");

            List<Movie> moviesToReturn = new List<Movie>();

            foreach (XmlNode node in nodes)
            {
                moviesToReturn.Add(new Movie(node.SelectSingleNode("genres").InnerText.ToString())
                {
                    imageLocation = node.SelectSingleNode("image").InnerText.ToString(),
                    mid = Int32.Parse(node.SelectSingleNode("movieid").InnerText.ToString()),
                    name = node.SelectSingleNode("name").InnerText.ToString(),
                    onlineRating = Double.Parse(node.SelectSingleNode("rating").InnerText.ToString()),
                    userRating = Double.Parse(node.SelectSingleNode("userrating").InnerText.ToString()),
                    year = node.SelectSingleNode("year").InnerText.ToString(),
                    description = node.SelectSingleNode("description").InnerText.ToString()
                });
                moviesToReturn[moviesToReturn.Count - 1].setSortName();
            }

            moviesToReturn = moviesToReturn.OrderBy(x => x.sortName).ToList();

            return moviesToReturn;
        }

        /// <summary>
        /// Acts as an interpreter if you're using this library from node.js (using edge)
        /// </summary>
        /// <param name="movies"></param>
        /// <returns></returns>
        public async Task<object> addMoviesNode(dynamic movies)
        {

            string errorString = "Everything is fine.";
            try
            {
                List<Movie> listToAdd = new List<Movie>();

                foreach (var item in movies.payload)
                {
                    Movie toAdd = new Movie()
                        {
                            name = item.name,
                            description = item.description,
                            genresCSV = item.genresCSV,
                            mid = item.mid,
                            userRating = item.userRating,
                            onlineRating = item.onlineRating,
                            poster = item.poster,
                            year = item.year,
                            imageLocation = item.imageLocation
                        };

                    if(!isMovieDuplicate(toAdd.mid))
                    {
                        listToAdd.Add(toAdd);
                    }
                }
                verifyUserFile();
                addMovies(listToAdd);
            }
                
            catch(Exception ex)
            {
                log.Error("Problem encountered: " + ex.ToString());
                //Do something with the exception
            }

            return new { messagePayload = errorString };
        }

        /// <summary>
        /// Returns an object that mimics type Movie, but is an abstract class (for json serialization).
        /// </summary>
        /// <param name="mid">List of movie IDs</param>
        /// <returns></returns>
        public async Task<object> getMoviesNode(dynamic mid)
        {
            List<object> toReturn = new List<object>();

            try
            {
                foreach (int item in mid.payload)
                {
                    foreach (var movie in allMoviesInXml())
                    {
                        if (item == movie.mid)
                        {
                            toReturn.Add(new
                                {
                                    name = movie.name,
                                    description = movie.description,
                                    genres = movie.genresCSV,
                                    movieid = movie.mid,
                                    userrating = movie.userRating,
                                    rating = movie.onlineRating,
                                    posternum = movie.poster,
                                    year = movie.year,
                                    image = movie.imageLocation
                                });
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("Got error: " + ex.ToString());
            }

            //return new { stuff = toReturn };
            return new {stuff = toReturn};
        }

        /// <summary>
        /// Returns true if the MID passed in already exists in the XML file.
        /// </summary>
        /// <param name="movieid">MID to check in the XML file</param>
        /// <returns></returns>
        public bool isMovieDuplicate(int movieid)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(getUserPath());

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/usersettings/movies/movie");

            foreach (XmlNode node in nodes)
            {
                if (node.SelectSingleNode("movieid").InnerText == movieid.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a collection of movies from the XML file.
        /// </summary>
        /// <param name="movies">Pass a list of fully populated movies.</param>
        public void removeMovies(List<Movie> movies)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(getUserPath());

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/usersettings/movies/movie");

            foreach (Movie movie in movies)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.SelectSingleNode("movieid").InnerText == movie.mid.ToString())
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }

            doc.Save(getUserPath());
        }
    }
}
