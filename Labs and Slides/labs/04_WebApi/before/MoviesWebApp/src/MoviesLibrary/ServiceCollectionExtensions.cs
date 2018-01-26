using MoviesLibrary;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMoviesLibrary(this IServiceCollection services, string url)
        {
            services.AddSingleton(new ServiceConfiguration { Url = url });
            services.AddTransient<MovieService>();
            services.AddTransient<ReviewService>();

            return services;
        }

        public static IServiceCollection AddAccessTokenAccessor<T>(this IServiceCollection services)
            where T : class, IAccessTokenAccessor
        {
            services.AddTransient<IAccessTokenAccessor, T>();

            return services;
        }
    }
}
