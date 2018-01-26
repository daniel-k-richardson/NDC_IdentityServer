using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesNativeClientApp
{
    public class MovieSearchResult
    {
        public string SearchTerm { get; set; }
        public int Count { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
    }
}
