using System.Diagnostics;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Infrastructure.Decorators;

public class LoggingDecorator<TRequest, TResponse>(
    IHandler<TRequest, TResponse> inner,
    ILogger<LoggingDecorator<TRequest, TResponse>> logger)
    : IHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("{RequestName} started", requestName);

        try
        {
            var response = await inner.HandleAsync(request, ct);
            stopwatch.Stop();

            if (response is Result { IsSuccess: false } result)
            {
                var hasFailureErrors = result.Errors.Any(e => e.Type == ErrorType.Failure);
                var logLevel = hasFailureErrors ? LogLevel.Warning : LogLevel.Information;
                var errors = string.Join(", ", result.Errors.Select(e => new {e.Code, e.Description}));

                // logger.LogWarning(
                //     "{RequestName} failed in {ElapsedMs}ms — {ErrorCount} error(s): {ErrorCodes}",
                //     requestName, stopwatch.ElapsedMilliseconds, result.Errors.Count, errors);
                logger.Log(logLevel, "{RequestName} failed in {ElapsedMs}ms — {ErrorCount} error(s): {errors}",
                    requestName, stopwatch.ElapsedMilliseconds, result.Errors.Count, errors);
            }
            else
            {
                logger.LogInformation(
                    "{RequestName} completed in {ElapsedMs}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(ex,
                "{RequestName} threw an unhandled exception after {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}