using System.Collections.Generic;

namespace MoviesLibrary
{
    public class GetMovieResult
    {
        public int Page { get; set; }
        public int Count { get; set; }
        public int? NextPage { get; set; }
        public int? PrevPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
    }
}
