using System.ComponentModel.DataAnnotations;
using ShikicinemaStatics.Posters.PosterListProviders;

namespace ShikicinemaStatics.Posters;

public record PostersLoaderOptions : IValidatableObject
{
    public const string SectionName = "PostersLoader";

    public bool Enabled { get; init; } = true;

    public TimeSpan QueriesInterval { get; init; } = TimeSpan.FromSeconds(1);

    public TimeSpan ScanInterval { get; init; } = TimeSpan.FromDays(1);

    public PosterListProviderStrategy ListProviderStrategy { get; init; } = PosterListProviderStrategy.StartToEndJpegAndWebp;

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

        if (ScanInterval <= TimeSpan.Zero)
        {
            yield return new ValidationResult("ScanInterval must be greater than zero", [nameof(ScanInterval)]);
        }

        if (ScanInterval.TotalMilliseconds > int.MaxValue)
        {
            // https://stackoverflow.com/questions/64553940/delaying-a-task-for-a-very-very-long-time
            yield return new ValidationResult("ScanInterval.TotalMilliseconds must be less than int.MaxValue", [nameof(ScanInterval)]);
        }
    }
}
