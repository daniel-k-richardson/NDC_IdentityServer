using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesLibrary;
using MoviesWebApi.Authorization;
using System.Threading.Tasks;

namespace MoviesWebApi.Controllers
{
    public class MovieController : ControllerBase
    {
        private readonly MovieService _movies;
        private readonly IAuthorizationService _authorization;

        public MovieController(MovieService movies, IAuthorizationService authorization)
        {
            _movies = movies;
            _authorization = authorization;
        }

        [HttpGet("movies/search")]
        [Authorize("SearchPolicy")]
        public IActionResult Search(string term)
        {
            var result = _movies.Search(term);
            return Ok(result);
        }

        [HttpGet("movies")]
        public IActionResult Get(int page)
        {
            var movies = _movies.GetAll(page);
            return Ok(movies);
        }

        [HttpGet("movie/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var movie = _movies.GetDetails(id);
            if (movie == null) return NotFound();

            movie.CanReview = (await _authorization.AuthorizeAsync(User, movie, MovieOperations.Review)).Succeeded;
            foreach(var review in movie.Reviews)
            {
                review.CanEdit = (await _authorization.AuthorizeAsync(User, review, ReviewOperations.Edit)).Succeeded;
            }

            return Ok(movie);
        }
    }
}
