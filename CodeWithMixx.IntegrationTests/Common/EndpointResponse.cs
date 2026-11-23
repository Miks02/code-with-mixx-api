
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.IntegrationTests.Common;

public record EndpointResponse<TData>(HttpResponseMessage Response, TData? Data, ProblemDetails? ProblemDetails) 
    where TData : class;
