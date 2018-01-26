using Microsoft.AspNetCore.Mvc;
using MoviesLibrary;
using MoviesWebApp.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MoviesWebApp.Controllers
{
    public class ReviewController : Controller
    {
        private MovieService _movies;
        private ReviewService _reviews;

        public ReviewController(ReviewService reviews, MovieService movies)
        {
            _reviews = reviews;
            _movies = movies;
        }

        IActionResult Error(IEnumerable<string> errors)
        {
            foreach (var err in errors)
            {
                ModelState.AddModelError("", err);
            }
            return View("Error");
        }

        [HttpGet]
        public async Task<IActionResult> New(int movieId)
        {
            var result = await _movies.GetDetailsAsync(movieId);
            if (result.IsAccessDenied)
            {
                return Forbid();
            }
            if (!result.Succeeded)
            {
                return Error(result.Errors);
            }

            var movie = result.Value;
            if (movie == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            if (!movie.CanReview)
            {
                return Forbid();
            }

            return View(new NewReviewModel() {
                MovieId = movieId,
                MovieTitle = movie.Title
            });
        }

        [HttpPost]
        public async Task<IActionResult> New(NewReviewModel model)
        {
            var result = await _movies.GetDetailsAsync(model.MovieId);
            if (result.IsAccessDenied)
            {
                return Forbid();
            }
            if (!result.Succeeded)
            {
                return Error(result.Errors);
            }

            var movie = result.Value;
            if (movie == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            if (!movie.CanReview)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var create = await _reviews.CreateAsync(model.MovieId, model.Stars, model.Comment);
                if (create.IsAccessDenied)
                {
                    return new ForbidResult();
                }

                if (create.Succeeded)
                {
                    return View("Success", new ReviewSuccessViewModel { MovieId = model.MovieId, Action = "Created" });
                }
                else
                {
                    foreach (var error in create.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            model.MovieTitle = movie.Title;

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _reviews.GetAsync(id);
            if (result.IsAccessDenied)
            {
                return Forbid();
            }
            if (!result.Succeeded)
            {
                return Error(result.Errors);
            }

            var review = result.Value;
            if (review == null)
            {
                return RedirectToAction("Index", "Movie");
            }

            if (!review.CanEdit)
            {
                return Forbid();
            }

            var movieResult = await _movies.GetDetailsAsync(review.MovieId);
            if(movieResult.IsAccessDenied)
            {
                return Forbid();
            }
            if (!movieResult.Succeeded)
            {
                return Error(movieResult.Errors);
            }

            var movie = movieResult.Value;

            var model = new EditReviewModel
            {
                ReviewId = id,
                Comment = review.Comment,
                Stars = review.Stars,
                MovieTitle = movie.Title,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditReviewModel model)
        {
            var result = await _reviews.GetAsync(model.ReviewId);
            if (result.IsAccessDenied)
            {
                return Forbid();
            }
            if (!result.Succeeded)
            {
                return Error(result.Errors);
            }

            var review = result.Value;
            if (review == null)
            {
                return RedirectToAction("Index", "Movie");
            }

            if (!review.CanEdit)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var update = await _reviews.UpdateAsync(model.ReviewId, model.Stars, model.Comment);
                if (update.IsAccessDenied)
                {
                    return Forbid();
                }
                if (!update.Succeeded)
                {
                    return Error(update.Errors);
                }

                if (update.Succeeded)
                {
                    return View("Success", new ReviewSuccessViewModel { MovieId = review.MovieId, Action = "Updated" });
                }
                else
                {
                    foreach (var error in update.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            var movieResult = await _movies.GetDetailsAsync(review.MovieId);
            if (movieResult.IsAccessDenied)
            {
                return Forbid();
            }
            if (!movieResult.Succeeded)
            {
                return Error(movieResult.Errors);
            }

            var movie = movieResult.Value;
            model.MovieTitle = movie.Title;

            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var result = await _reviews.GetAsync(reviewId);
            if (result.IsAccessDenied)
            {
                return Forbid();
            }
            if (!result.Succeeded)
            {
                return Error(result.Errors);
            }

            var review = result.Value;
            if (review == null)
            {
                return RedirectToAction("Index", "Movie");
            }

            if (!review.CanEdit)
            {
                return Forbid();
            }

            var deleteResult = await _reviews.DeleteAsync(reviewId);
            if (deleteResult.Succeeded)
            {
                return View("Success", new ReviewSuccessViewModel { MovieId = review.MovieId, Action = "Deleted" });
            }

            return RedirectToAction("Edit", new { id = reviewId });
        }
    }
}
