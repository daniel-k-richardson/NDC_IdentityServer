using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesLibrary;
using MoviesWebApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebApi.Controllers
{
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviews;
        private readonly MovieService _movies;
        private readonly IAuthorizationService _authorization;

        public ReviewController(ReviewService reviews, MovieService movies, IAuthorizationService authorization)
        {
            _reviews = reviews;
            _movies = movies;
            _authorization = authorization;
        }

        [HttpPost("movie/{movieId:int}/reviews")]
        public async Task<IActionResult> Post(int movieId, [FromBody] ReviewUpdateModel model)
        {
            var movie = _movies.GetDetails(movieId);
            if (movie == null) return NotFound();

            if (!(await _authorization.AuthorizeAsync(User, movie, Authorization.MovieOperations.Review)).Succeeded)
            {
                return Forbid();
            }

            var userId = User.FindFirst("sub")?.Value;

            var result = _reviews.Create(movieId, model.Stars, model.Comment, userId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.ToArray());
            }

            return NoContent();
        }

        [HttpGet("review/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var review = _reviews.Get(id);
            if (review == null) return NotFound();

            review.CanEdit = (await _authorization.AuthorizeAsync(User, review, Authorization.ReviewOperations.Edit)).Succeeded;

            return Ok(review);
        }

        [HttpPut("review/{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] ReviewUpdateModel model)
        {
            var review = _reviews.Get(id);
            if (review == null) return NotFound();

            if (!(await _authorization.AuthorizeAsync(User, review, Authorization.ReviewOperations.Edit)).Succeeded)
            {
                return Forbid();
            }

            var result = _reviews.Update(id, model.Stars, model.Comment);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.ToArray());
            }

            return NoContent();
        }

        [HttpDelete("review/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = _reviews.Get(id);
            if (review == null) return NotFound();

            if (!(await _authorization.AuthorizeAsync(User, review, Authorization.ReviewOperations.Edit)).Succeeded)
            {
                return Forbid();
            }

            var result = _reviews.Delete(id);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.ToArray());
            }

            return NoContent();
        }
    }
}
