using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoviesWebApp.ViewModels
{
    public class NewReviewModel : IValidatableObject
    {
        public string MovieTitle { get; set; }
        public int MovieId { get; set; }

        public int? Stars { get; set; }
        public string Comment { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Stars == null && String.IsNullOrWhiteSpace(Comment))
            {
                yield return new ValidationResult("Stars or Comment is required");
            }

            if (Stars < 1 || Stars > 5)
            {
                yield return new ValidationResult("Stars must be between 1 and 5");
            }
        }
    }
}
