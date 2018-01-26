using System.Collections.Generic;

namespace MoviesLibrary
{
    public class MovieSearchResult
    {
        public string SearchTerm { get; set; }
        public int Count { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
    }
}
