using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using MoviesLibrary;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMoviesLibrary();

            services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            
            // TODO: add the OIDC authentication handler
            // TODO: configure the TokenValidationParameters to use "name" and "role" claims
            // TODO: add events to handle OnTicketReceived to add app-specific claims

            
            // TODO: add Google authentication handler
            // google settings
            // ClientId = "998042782978-lrga3i7tf8g6eotqv3ltjhqd2bguhnf4.apps.googleusercontent.com",
            // ClientSecret = "lAVx368q3GDXZS_dlrrntrDN"


            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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

            // Add framework services.
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
