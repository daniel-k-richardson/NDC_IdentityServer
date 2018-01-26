using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MoviesLibrary;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebApp.Authorization
{
    public class MovieOperations
    {
        public static readonly OperationAuthorizationRequirement Review =
            new OperationAuthorizationRequirement { Name = "Review" };
    }

    public class MovieAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, MovieDetails>
    {
        private ReviewPermissionService _reviewPermissions;

        public MovieAuthorizationHandler(ReviewPermissionService reviewPermissions)
        {
            _reviewPermissions = reviewPermissions;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            OperationAuthorizationRequirement requirement, 
            MovieDetails movie)
        {
            if (requirement == MovieOperations.Review)
            {
                if (context.User.HasClaim("role", "Reviewer"))
                {
                    var allowed = _reviewPermissions.GetAllowedCountries(context.User);
                    if (allowed.Contains(movie.CountryName))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}
