using System.ComponentModel.DataAnnotations;

namespace ShikicinemaStatics.Posters;

public class PostersLoaderOptions : IValidatableObject
{
    public const string SectionName = "PostersLoader";

    public bool Enabled { get; set; } = true;

    public TimeSpan QueriesInterval { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan ScanInterval { get; set; } = TimeSpan.FromHours(2);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (QueriesInterval < TimeSpan.Zero)
        {
            yield return new ValidationResult("QueriesInterval must be positive", [nameof(QueriesInterval)]);
        }

        if (ScanInterval <= TimeSpan.Zero)
        {
            yield return new ValidationResult("ScanInterval must be greater than zero", [nameof(ScanInterval)]);
        }
    }
}
