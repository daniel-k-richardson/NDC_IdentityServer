using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MoviesLibrary
{
    public class MovieService
    {
        const string GetAllUrl = "movies?page={0}";
        const string SearchUrl = "movies/search?term={0}";
        const string DetailsUrl = "movie/{0}";

        private readonly ServiceConfiguration _config;
        private readonly IAccessTokenAccessor _accessToken;

        public MovieService(ServiceConfiguration config, IAccessTokenAccessor accessToken)
        {
            _config = config;
            _accessToken = accessToken;
        }

        async Task<HttpClient> GetHttpClientAsync()
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(_config.Url);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var accessToken = await _accessToken.GetAccessTokenAsync();
            if (accessToken != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return client;
        }

        public async Task<Result<GetMovieResult>> GetAllAsync(int page)
        {
            var url = String.Format(GetAllUrl, page);

            var client = await GetHttpClientAsync();
            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Result<GetMovieResult>.AccessDenied;
            }

            if (!response.IsSuccessStatusCode)
            {
                return new Result<GetMovieResult>(await response.Content.ReadAsJsonAsync<string[]>());
            }

            return new Result<GetMovieResult>(await response.Content.ReadAsJsonAsync<GetMovieResult>());
        }

        public async Task<Result<MovieSearchResult>> SearchAsync(string term)
        {
            if (String.IsNullOrEmpty(term))
            {
                return new Result<MovieSearchResult>(new MovieSearchResult());
            }

            var url = String.Format(SearchUrl, UrlEncoder.Default.Encode(term));

            var client = await GetHttpClientAsync();
            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Result<MovieSearchResult>.AccessDenied;
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength > 0)
                {
                    return new Result<MovieSearchResult>(await response.Content.ReadAsJsonAsync<string[]>());
                }

                return new Result<MovieSearchResult>(response.ReasonPhrase);
            }

            return new Result<MovieSearchResult>(await response.Content.ReadAsJsonAsync<MovieSearchResult>());
        }

        public async Task<Result<MovieDetails>> GetDetailsAsync(int id)
        {
            var url = String.Format(DetailsUrl, id);

            var client = await GetHttpClientAsync();
            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Result<MovieDetails>.AccessDenied;
            }

            if (!response.IsSuccessStatusCode)
            {
                return new Result<MovieDetails>(await response.Content.ReadAsJsonAsync<string[]>());
            }

            return new Result<MovieDetails>(await response.Content.ReadAsJsonAsync<MovieDetails>());
        }
    }
}
