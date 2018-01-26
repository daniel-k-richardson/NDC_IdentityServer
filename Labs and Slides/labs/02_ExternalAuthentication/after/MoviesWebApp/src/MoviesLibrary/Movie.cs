using System;
using System.Collections.Generic;
using System.Linq;

namespace MoviesLibrary
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

    public class MovieReview
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
        public int? Stars { get; set; }
        public DateTime ReviewDate { get; set; }
    }

    public class MovieDetails : Movie
    {
        public IEnumerable<MovieReview> Reviews { get; set; }
        public double? AverageStars
        {
            get
            {
                if (Reviews == null || !Reviews.Any()) return null;
                return Reviews.Select(x => x.Stars).Average();
            }
        }
    }
}
