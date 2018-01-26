using System;
using System.Linq;

namespace MoviesLibrary
{
    public class MovieService
    {
        public GetMovieResult GetAll(int page)
        {
            int pageSize = 20;

            var movies = Internal.Data.MovieData;

            int totalRows = movies.Count();
            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

            int currentPage = page;
            if (currentPage < 1) currentPage = 1;
            if (currentPage > totalPages) currentPage = totalPages;

            var result = new GetMovieResult();

            result.Page = currentPage;
            result.NextPage = currentPage < totalPages ? currentPage + 1 : (int?)null;
            result.PrevPage = currentPage > 1 ? currentPage - 1 : (int?)null;

            result.TotalPages = totalPages;
            result.TotalCount = totalRows;

            int startRow = (currentPage - 1) * pageSize;
            var list =
                (from movie in movies.OrderBy(x => x.Id).Skip(startRow).Take(pageSize)
                 select new Movie
                 {
                     Id = movie.Id,
                     Title = movie.Title,
                     Description = movie.Description,
                     Rating = movie.Rating,
                     Year = movie.Year,
                     PosterName = movie.PosterName,
                     DirectorName = Internal.Data.DirectorData.SingleOrDefault(x => x.Id == movie.DirectorId)?.Name,
                     CountryName = Internal.Data.CountryData.SingleOrDefault(x => x.Id == movie.CountryId)?.Name
                 }).ToArray();

            result.Movies = list;
            result.Count = list.Length;

            return result;
        }

        public MovieSearchResult Search(string term)
        {
            if (String.IsNullOrWhiteSpace(term))
            {
                return new MovieSearchResult();
            }

            term = term.ToLowerInvariant();

            var movies = Internal.Data.MovieData;

            var list =
                (from movie in movies
                 where movie.Title.ToLowerInvariant().Contains(term)
                 orderby movie.Id
                 select new Movie
                 {
                     Id = movie.Id,
                     Title = movie.Title,
                     Description = movie.Description,
                     Rating = movie.Rating,
                     Year = movie.Year,
                     PosterName = movie.PosterName,
                     DirectorName = Internal.Data.DirectorData.SingleOrDefault(x => x.Id == movie.DirectorId)?.Name,
                     CountryName = Internal.Data.CountryData.SingleOrDefault(x => x.Id == movie.CountryId)?.Name
                 }).ToArray();

            return new MovieSearchResult
            {
                SearchTerm = term,
                Count = list.Length,
                Movies = list
            };
        }

        public MovieDetails GetDetails(int id)
        {
            var movie = Internal.Data.MovieData.FirstOrDefault(x => x.Id == id);
            if (movie == null) return null;

            var result = new MovieDetails
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Rating = movie.Rating,
                Year = movie.Year,
                PosterName = movie.PosterName,
                DirectorName = Internal.Data.DirectorData.SingleOrDefault(x => x.Id == movie.DirectorId)?.Name,
                CountryName = Internal.Data.CountryData.SingleOrDefault(x => x.Id == movie.CountryId)?.Name
            };

            var reviews =
                from review in Internal.Data.ReviewData
                where review.MovieId == id
                orderby review.ReviewDate
                select new MovieReview
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    ReviewDate = review.ReviewDate,
                    Comment = review.Comment,
                    Stars = review.Stars
                };
            result.Reviews = reviews.ToArray();

            return result;
        }
    }
}
