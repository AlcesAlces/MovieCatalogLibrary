using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.Misc
{
    public static class MiscHelpers
    {
        /// <summary>
        /// Creates a movie object from a dynamic object. Expecting a serialized JSON object
        /// plz don't pass bad stuff in here. You might end the world.
        /// </summary>
        /// <param name="movies">Serialized JSON object. Follow the structure in the method.</param>
        /// <returns></returns>
        public static List<Movie> moivesFromDynamic(dynamic movies)
        {

            List<Movie> listToAdd = new List<Movie>();
            try
            {
                foreach (dynamic item in movies.payload)
                {

                    Movie toAdd = new Movie(item);

                    listToAdd.Add(toAdd);
                }
            }
            catch(Exception ex)
            {
                int i = 0;
            }

            return listToAdd;
        }
    }
}
