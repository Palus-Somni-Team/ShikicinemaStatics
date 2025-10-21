using Polly;

namespace ShikicinemaStatics.Polly;

public class SharedPollyContext : IDisposable
{
    private readonly ResilienceContext _pollyContext;

    public ResilienceContext PollyContext => _pollyContext;

    public SharedPollyContext(string operationName, CancellationToken token)
    {
        _pollyContext = ResilienceContextPool.Shared.Get(operationName, token);
    }

    public void Dispose()
    {
        ResilienceContextPool.Shared.Return(_pollyContext);
    }
}
