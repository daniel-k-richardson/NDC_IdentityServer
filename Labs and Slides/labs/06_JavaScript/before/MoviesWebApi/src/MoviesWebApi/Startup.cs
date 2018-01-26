using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MoviesWebApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
			// TODO: add CORS and configure a policy for the movies web app
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.Authority = "http://localhost:1941/";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "movie_api";
                });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddMoviesLibrary();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SearchPolicy", builder =>
                {
                    builder.RequireAuthenticatedUser();
                    builder.RequireAssertion(ctx =>
                    {
                        if (ctx.User.HasClaim("role", "Admin") ||
                            ctx.User.HasClaim("role", "Customer"))
                        {
                            return true;
                        }
                        return false;
                    });
                });
            });

            services.AddTransient<IAuthorizationHandler, Authorization.ReviewAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, Authorization.MovieAuthorizationHandler>();

			services.AddMvc(options=>
            {
                var builder = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser();
                options.Filters.Add(new AuthorizeFilter(builder.Build()));
            });
        }

        public void Configure(IApplicationBuilder app)
        {
			// TODO: add the CORS middleware and use the policy configured above
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
