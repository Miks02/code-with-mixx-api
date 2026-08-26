namespace CodeWithMixx.API.Common.Interfaces;

public interface IHandler;

public interface IHandler<in TRequest, TResponse> : IHandler
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}