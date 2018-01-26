using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesNativeClientApp
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public string Rating { get; set; }
        public string PosterName { get; set; }
        public string DirectorName { get; set; }
        public string CountryName { get; set; }
    }
}
