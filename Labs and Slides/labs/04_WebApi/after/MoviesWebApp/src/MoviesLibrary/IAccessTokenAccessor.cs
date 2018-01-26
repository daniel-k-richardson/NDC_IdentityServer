using System.Threading.Tasks;

namespace MoviesLibrary
{
    public interface IAccessTokenAccessor
    {
        Task<string> GetAccessTokenAsync();
    }
}
