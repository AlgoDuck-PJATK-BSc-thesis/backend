using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Shared.Attributes;

public class MaxFileSizeAttribute(long maxBytes) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var tooBig = (value switch
        {
            IFormFile f => [f],
            IFormFileCollection fc => fc.ToList(),
            _ => []
        }).FirstOrDefault(f => f.Length > maxBytes);
        
        if (tooBig != null)
            return new ValidationResult($"File '{tooBig.FileName}' exceeds max size of {maxBytes} bytes");
        
        return ValidationResult.Success;
    }
}