using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Shared.Attributes;

public class MaxFileCountAttribute(int maxCount) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is IFormFileCollection files && files.Count > maxCount)
            return new ValidationResult($"Maximum {maxCount} files allowed");
        return ValidationResult.Success;
    }
}