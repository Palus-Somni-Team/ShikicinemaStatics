using System.ComponentModel.DataAnnotations;

namespace ShikicinemaStatics;

public class StaticFileOptions : IValidatableObject
{
    public const string SectionName = "ShikicinemaStatics";
    
    public string PhysicalPath { get; init; } = null!;
    
    public string RequestPath { get; init; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(PhysicalPath))
        {
            yield return new ValidationResult("PhysicalPath is required.", [nameof(PhysicalPath)]);
        }

        if (!Directory.Exists(PhysicalPath))
        {
            yield return new ValidationResult("PhysicalPath directory is not found.", [nameof(PhysicalPath)]);
        }

        if (string.IsNullOrEmpty(RequestPath))
        {
            yield return new ValidationResult("RequestPath is required.", [nameof(PhysicalPath)]);
        }

        if (!RequestPath.StartsWith("/"))
        {
            yield return new ValidationResult("RequestPath must start with '/'.", [nameof(RequestPath)]);
        }
    }
}
