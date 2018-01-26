using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // TODO: apply the search authorization policy
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
