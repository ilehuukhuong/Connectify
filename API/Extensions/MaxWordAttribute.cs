using System.ComponentModel.DataAnnotations;

namespace API.Extensions
{
    public class MaxWordAttribute : ValidationAttribute
    {
        private readonly int _maxWords;

        public MaxWordAttribute(int maxWords)
        {
            _maxWords = maxWords;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var wordCount = value.ToString().Split(' ').Length;
                if (wordCount > _maxWords)
                {
                    return new ValidationResult($"The {validationContext.DisplayName.ToLower()} must be less than {_maxWords} words.");
                }
            }

            return ValidationResult.Success;
        }
    }
}