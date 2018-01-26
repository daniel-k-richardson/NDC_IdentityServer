using System.Collections.Generic;
using System.Security.Claims;

namespace MoviesLibrary
{
    public class MovieIdentityService
    {
        public IEnumerable<Claim> GetClaimsForUser(string subject)
        {
            var claims = new List<Claim>();

            if (subject == "user1")
            {
                claims.Add(new Claim("role", "Reviewer"));
                claims.Add(new Claim("name", "User One"));
            }
            else if (subject == "user2")
            {
                claims.Add(new Claim("role", "Reviewer"));
                claims.Add(new Claim("name", "User Two"));
            }
            else if (subject == "user3")
            {
                claims.Add(new Claim("role", "Reviewer"));
                claims.Add(new Claim("name", "User Three"));
            }
            else if (subject == "user4")
            {
                claims.Add(new Claim("role", "Customer"));
                claims.Add(new Claim("name", "User Four"));
            }
            else if (subject == "user5")
            {
                claims.Add(new Claim("role", "Admin"));
                claims.Add(new Claim("name", "User Five"));
            }
            else
            {
                claims.Add(new Claim("name", subject));
            }

            return claims;
        }
    }
}
