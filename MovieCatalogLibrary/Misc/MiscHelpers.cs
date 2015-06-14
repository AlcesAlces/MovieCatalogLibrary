using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.Misc
{
    public static class MiscHelpers
    {
        public static List<Movie> moivesFromDynamic(dynamic movies)
        {

            List<Movie> listToAdd = new List<Movie>();

            foreach (var item in movies.payload)
            {
                Movie toAdd = new Movie()
                {
                    name = item.name,
                    description = item.description,
                    genresCSV = item.genres,
                    mid = Int32.Parse(item.movieid),
                    userRating = double.Parse(item.userrating),
                    onlineRating = double.Parse(item.rating),
                    poster = Int32.Parse(item.posternum),
                    year = item.year,
                    imageLocation = item.image
                };

                listToAdd.Add(toAdd);
            }

            return listToAdd;
        }
    }
}
