using Microsoft.AspNetCore.Mvc;
using MoviesLibrary;
using Microsoft.AspNetCore.Authorization;

namespace MoviesWebApp.Controllers
{
    public class MovieController : Controller
    {
        private MovieService _movies;

        public MovieController(MovieService movies)
        {
            _movies = movies;
        }

        public IActionResult Index(int page = 1)
        {
            var movies = _movies.GetAll(page);
            return View(movies);
        }

        [Authorize("SearchPolicy")]
        public IActionResult Search(string searchTerm = null)
        {
            var result = searchTerm != null ? _movies.Search(searchTerm) : new MovieSearchResult();
            return View(result);
        }

        public IActionResult Details(int id)
        {
            var details = _movies.GetDetails(id);
            if (details == null)
            {
                return RedirectToAction("Index");
            }

            return View(details);
        }
    }
}
