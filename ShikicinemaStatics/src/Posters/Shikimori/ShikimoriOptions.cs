using System.ComponentModel.DataAnnotations;

namespace ShikicinemaStatics.Posters.Shikimori;

public class ShikimoriOptions : IValidatableObject
{
    public const string SectionName = "Shikimori";
    
    public string Host { get; set; } = "https://shikimori.one";
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Host))
        {
            yield return new ValidationResult("Host is required.", [nameof(Host)]);
        }

        if (!Uri.TryCreate(Host, UriKind.Absolute, out _))
        {
            yield return new ValidationResult("Host must be valid absolute url.", [nameof(Host)]);
        }
    }
}