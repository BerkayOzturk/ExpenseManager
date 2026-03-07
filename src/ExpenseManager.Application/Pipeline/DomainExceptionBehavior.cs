using ExpenseManager.Application.Common.Exceptions;
using ExpenseManager.Domain.Common;
using MediatR;

namespace ExpenseManager.Application.Pipeline;

/// <summary>
/// Converts Domain exceptions to Application exceptions so outer layers (API) need not reference Domain.
/// </summary>
public sealed class DomainExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)
        {
            throw new DomainRuleException(ex.Message);
        }
    }
}
