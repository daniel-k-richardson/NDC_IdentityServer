using System.Collections.Generic;
using System.Linq;

namespace MoviesLibrary
{
    public class Result
    {
        public static readonly Result Success = new Result(null);

        public Result(params string[] error)
        {
            Errors = error;
        }

        public IEnumerable<string> Errors { get; private set; }
        public bool Succeeded
        {
            get
            {
                return Errors == null || !Errors.Any();
            }
        }
    }
}
