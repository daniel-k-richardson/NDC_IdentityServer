using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MoviesWebApi;

namespace MoviesWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
