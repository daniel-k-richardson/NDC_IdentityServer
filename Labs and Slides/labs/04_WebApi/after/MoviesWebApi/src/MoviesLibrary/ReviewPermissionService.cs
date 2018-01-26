using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MoviesLibrary
{
    public class ReviewPermissionService
    {
        public IEnumerable<string> GetAllowedCountries(ClaimsPrincipal user)
        {
            var sub = user.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            if (sub != null)
            {
                var last = sub[sub.Length - 1];
                int id;
                if (Int32.TryParse(last.ToString(), out id))
                {
                    var countries = Internal.Data.CountryData;
                    return countries.Where(x => x.Id % id == 0).Select(x => x.Name);
                }
            }

            return Enumerable.Empty<string>();
        }
    }
}
