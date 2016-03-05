using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatTmdb.V3;

namespace MovieCatalogLibrary
{
    public class Movie : IMovie
    {
        public string name { get; set; }
        public string year { get; set; }
        public int mid { get; set; }
        public string imageLocation { get; set; }
        public double onlineRating { get; set; }
        public double userRating = 0;
        public string description { get; set; }
        public List<MovieGenre> genres = new List<MovieGenre>();
        public string genresCSV { get; set; }
        //Use this name to sort by, it will exclude "the" in a title.
        public string sortName { get; set; }
        public int poster = 0;

        /// <summary>
        /// Use to parse a CSV string.
        /// </summary>
        /// <param name="toParse"></param>
        public Movie(string toParse)
        {
            setGenreCommaSeperated(toParse);
            setSortName();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Movie()
        {
            setSortName();
        }

        /// <summary>
        /// From dynamic object.
        /// </summary>
        /// <param name="dyn"></param>
        public Movie(dynamic item)
        {
            name = item.name.Value;
            description = item.description.Value;
            //TODO: FIX THIS!
            //genresCSV = item.genres.Value;
            mid = Int32.Parse(item.mid.Value.ToString());
            userRating = double.Parse(item.userRating.Value.ToString());
            onlineRating = double.Parse(item.onlineRating.Value.ToString());
            poster = Int32.Parse(item.poster.Value.ToString());
            year = item.year.Value;
            imageLocation = item.imageLocation.Value;
        }

        /// <summary>
        /// Constructor to interface directly with the TMDB movie type.
        /// </summary>
        /// <param name="toSet"></param>
        public Movie(TmdbMovie movie)
        {
            TMDBHelper tmdbHelper = new TMDBHelper();
            TmdbMovieImages image = tmdbHelper.getImagesById(movie.id);

            string posterLocation = "";

            try
            {
                posterLocation = image.posters[0].file_path;
            }

            catch
            {
                posterLocation = "NONE";
            }

            description = movie.overview;
            imageLocation = posterLocation;
            mid = movie.id;
            name = movie.title;
            onlineRating = movie.vote_average;
            userRating = 0.0;
            year = movie.release_date;
            genres = movie.genres;
        }

        public string getGenreCommaSeperated()
        {
            string toReturn = "";

            if (genres.Count == 0)
            {
                return genresCSV;
            }

            else
            {

                foreach (MovieGenre genre in genres)
                {
                    toReturn += genre.name + ",";
                }

                if (toReturn == "")
                {
                    return toReturn;
                }
                else
                {
                    return toReturn.Substring(0, toReturn.Count() - 1);
                }
            }
        }

        public void setGenreCommaSeperated(string toBeParsed)
        {
            List<string> splitString = toBeParsed.Split(',').ToList();

            foreach (string item in splitString)
            {
                genres.Add(new MovieGenre
                {
                    name = item
                });
            }
        }

        public void setSortName()
        {
            if (name != null)
            {
                if (name.Split(' ')[0].ToLower() == "the")
                {
                    var tmp = name.Split(' ').ToList();
                    tmp.RemoveAt(0);
                    sortName = String.Join(" ", tmp);
                }

                else
                {
                    sortName = name;
                }
            }
        }
    }
}
