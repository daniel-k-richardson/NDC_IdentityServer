using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using IdentityModel.Client;
using System.Diagnostics;
using System.Runtime.InteropServices;
using IdentityModel;
using System.Text;
using System.Security.Cryptography;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;

namespace MoviesNativeClientApp
{
    public class Program
    {
        const string searchUrl = "http://localhost:53223/movies/search?term={0}";
        const string authorityUrl = "http://localhost:1941/";

        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            Console.WriteLine("Welcome to the movie search app!");
            Console.WriteLine("Search for a movie!");
            Console.WriteLine();

            var accessToken = await PromptForPasswordAsync();
            //var accessToken = await LaunchBrowserForTokenAsync();
            if (accessToken == null) return;

            await RunSearchLoopAsync(accessToken);
        }

        static async Task RunSearchLoopAsync(string accessToken)
        {
            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter the search term (or just enter to exit the application): ");

                var term = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(term)) return;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var url = String.Format(searchUrl, WebUtility.UrlEncode(term));
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<MovieSearchResult>(json);
                    ShowResult(result);
                }
                else
                {
                    Console.WriteLine($"Error: {(int)response.StatusCode}");
                    break;
                }
            }
        }

        const int TitleWidth = 30;
        const int YearWidth = 6;
        const int RatingWidth = 8;
        const int CountryWidth = 20;

        const string Separator = "  ";

        private static void ShowResult(MovieSearchResult result)
        {
            Console.WriteLine($"{result.Count} results");
            foreach(var item in result.Movies)
            {
                Console.Write("\t");

                Console.Write(Fit(item.Title, TitleWidth));
                Console.Write(Separator);

                Console.Write(Fit(item.Year.ToString(), YearWidth));
                Console.Write(Separator);

                Console.Write(Fit(item.Rating, RatingWidth));
                Console.Write(Separator);

                Console.Write(Fit(item.CountryName, CountryWidth));
                Console.WriteLine();
            }
        }

        static string Fit(string value, int length)
        {
            value = value ?? String.Empty;
            value = value.Trim();

            if (value.Length >= length)
            {
                value = value.Substring(0, length - 3);
                value += "...";
            }
            else if (value.Length < length)
            {
                value += new String(' ', length - value.Length);
            }

            return value;
        }

        private static async Task<string> PromptForPasswordAsync()
        {
            Console.WriteLine("Enter your credentials.");
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            // TODO: use DiscoveryClient to get metadata

            // TODO: use TokenClient to request resource owner password

            // TODO: if token result IsError then show the error info and return null from this method


            Console.WriteLine();

            // TODO: return the access token (instead of null)
            return null;
        }

        private static async Task<string> LaunchBrowserForTokenAsync()
        {
            var browser = new SystemBrowser(12345, "/native_client_callback");

            // TODO: setup the OidcClientOptions

            // TODO: create the OidcClient

            // TODO: call LoginAsync to get the result

            // TODO: if result is not Success then show the error info and return null from this method


            // TODO: return the access token (instead of null)
            return null;
        }
    }
}
