using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MoviesLibrary
{
    public class ReviewService
    {
        const string CreateUrl = "movie/{0}/reviews";
        const string GetUrl = "review/{0}";
        const string UpdateUrl = "review/{0}";
        const string DeleteUrl = "review/{0}";

        private readonly ServiceConfiguration _config;
        private readonly IAccessTokenAccessor _accessToken;

        public ReviewService(ServiceConfiguration config, IAccessTokenAccessor accessToken)
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

        public async Task<Result> CreateAsync(int movieId, int? stars, string comment)
        {
            var url = String.Format(CreateUrl, movieId);

            var client = await GetHttpClientAsync();
            var response = await client.PostAsJsonAsync(url, new { stars, comment });

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Result.AccessDenied;
            }

            if (!response.IsSuccessStatusCode)
            {
                return new Result(await response.Content.ReadAsJsonAsync<string[]>());
            }

            return Result.Success;
        }

        public async Task<Result<Review>> GetAsync(int id)
        {
            var url = String.Format(GetUrl, id);

            var client = await GetHttpClientAsync();
            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Result<Review>.AccessDenied;
            }

            if (!response.IsSuccessStatusCode)
            {
                return new Result<Review>(await response.Content.ReadAsJsonAsync<string[]>());
            }

            return new Result<Review>(await response.Content.ReadAsJsonAsync<Review>());
        }

        public async Task<Result> UpdateAsync(int reviewId, int? stars, string comment)
        {
            var url = String.Format(UpdateUrl, reviewId);

            var client = await GetHttpClientAsync();
            var response = await client.PutAsJsonAsync(url, new { stars, comment });

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Result.AccessDenied;
            }

            if (!response.IsSuccessStatusCode)
            {
                return new Result(await response.Content.ReadAsJsonAsync<string[]>());
            }

            return Result.Success;
        }

        public async Task<Result> DeleteAsync(int reviewId)
        {
            var url = String.Format(DeleteUrl, reviewId);

            var client = await GetHttpClientAsync();
            var response = await client.DeleteAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Result.AccessDenied;
            }

            if (!response.IsSuccessStatusCode)
            {
                return new Result(await response.Content.ReadAsJsonAsync<string[]>());
            }

            return Result.Success;
        }
    }
}
