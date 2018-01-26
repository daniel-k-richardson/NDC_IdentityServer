using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using MoviesLibrary;
using System.Threading.Tasks;

namespace MoviesWebApp
{
    public class AccessTokenAccessor : IAccessTokenAccessor
    {
        private readonly IHttpContextAccessor _context;

        public AccessTokenAccessor(IHttpContextAccessor context)
        {
            _context = context;
        }

        public Task<string> GetAccessTokenAsync()
        {
            return _context.HttpContext.GetTokenAsync("access_token");
        }
    }
}
