using System.ComponentModel.DataAnnotations;
using ShikicinemaStatics.Posters.PosterListProviders;

namespace ShikicinemaStatics.Posters;

public record PostersLoaderOptions : IValidatableObject
{
    public const string SectionName = "PostersLoader";

    public TimeSpan QueriesInterval { get; init; } = TimeSpan.FromSeconds(1);

    public PosterListProviderStrategy ListProviderStrategy { get; init; } = PosterListProviderStrategy.StartToEnd;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (QueriesInterval < TimeSpan.Zero)
        {
            yield return new ValidationResult("QueriesInterval must be positive", [nameof(QueriesInterval)]);
        }

        if (QueriesInterval >= TimeSpan.FromSeconds(60))
        {
            yield return new ValidationResult("QueriesInterval must be less than 60 seconds", [nameof(QueriesInterval)]);
        }
    }
}
