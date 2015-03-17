using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatTmdb.V3;

namespace MovieCatalogLibrary
{
    public class TMDBHelper
    {
        private static readonly string apikey = "56587e13dc926d742e62c09151418bd3";
        private static readonly string language = "en";
        private static Tmdb api = new Tmdb(apikey, language);

        public List<MovieResult> movieResultsBySearch(string searchTerm)
        {
            if (searchTerm.Count() != 0)
            {
                TmdbMovieSearch results = api.SearchMovie(searchTerm, 1);
                return (api.SearchMovie(searchTerm, 1) as TmdbMovieSearch).results;
            }

            else
            {
                return new List<MovieResult>();
            }
        }

        public TmdbMovie getTmdbMovieById(int mid)
        {
            return api.GetMovieInfo(mid);
        }

        public TmdbMovieImages getImagesById(int mid)
        {
            return api.GetMovieImages(mid);
        }
    }
}
