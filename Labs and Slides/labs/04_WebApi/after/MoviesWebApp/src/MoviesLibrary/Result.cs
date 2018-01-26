using System.Collections.Generic;
using System.Linq;

namespace MoviesLibrary
{
    public class Result
    {
        public static readonly Result Success = new Result(new string[0]);
        public static readonly Result AccessDenied = new Result("Access Denied");

        public Result(params string[] error)
        {
            Errors = error;
            if (error == null)
            {
                Errors = new string[] { "Unexpected Error" };
            }
        }

        public IEnumerable<string> Errors { get; private set; }
        public bool Succeeded
        {
            get
            {
                return Errors == null || !Errors.Any();
            }
        }

        public bool IsAccessDenied
        {
            get
            {
                return this == AccessDenied;
            }
        }
    }

    public class Result<T> : Result
    {
        public new static readonly Result<T> AccessDenied = new Result<T>("Access Denied");

        public Result(params string[] error) : base(error)
        {
        }

        public Result(T value)
        {
            Value = value;
        }

        public new bool IsAccessDenied
        {
            get
            {
                return this == AccessDenied;
            }
        }

        public T Value { get; private set; }
    }
}
