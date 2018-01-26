using System;
using System.Collections.Generic;
using System.Linq;

namespace MoviesLibrary
{
    public class ReviewService
    {
        public Result Create(int movieId, int? stars, string comment, string userId)
        {
            var errors = new List<string>();
            if (!Internal.Data.MovieData.Any(x=>x.Id == movieId))
            {
                errors.Add("Invalid movie");
            }

            if (stars == null && String.IsNullOrWhiteSpace(comment))
            {
                errors.Add("Stars or comment is required");
            }

            if (stars < 1 || stars > 5)
            {
                errors.Add("Stars must be between 1 and 5");
            }

            if (String.IsNullOrWhiteSpace(userId))
            {
                errors.Add("Invalid user");
            }

            if (errors.Any())
            {
                return new Result(errors.ToArray());
            }

            var review = new Review
            {
                Comment = comment,
                Id = Internal.Data.ReviewData.Max(x=>x.Id) + 1, // yes, hacky
                MovieId = movieId,
                ReviewDate = DateTime.Now,
                Stars = stars,
                UserId = userId,
            };
            Internal.Data.ReviewData.Add(review);

            return Result.Success;
        }

        public Review Get(int id)
        {
            return Internal.Data.ReviewData.FirstOrDefault(x => x.Id == id);
        }

        public Result Update(int reviewId, int? stars, string comment)
        {
            var errors = new List<string>();
            var review = Internal.Data.ReviewData.FirstOrDefault(x => x.Id == reviewId);
            if (review == null)
            {
                errors.Add("Invalid review");
            }

            if (stars == null && String.IsNullOrWhiteSpace(comment))
            {
                errors.Add("Stars or comment is required");
            }

            if (errors.Any())
            {
                return new Result(errors.ToArray());
            }

            review.Stars = stars;
            review.Comment = comment;

            return Result.Success;
        }

        public Result Delete(int reviewId)
        {
            var errors = new List<string>();
            var review = Internal.Data.ReviewData.FirstOrDefault(x => x.Id == reviewId);
            if (review == null)
            {
                errors.Add("Invalid review");
            }

            Internal.Data.ReviewData.Remove(review);

            return Result.Success;
        }
    }
}
