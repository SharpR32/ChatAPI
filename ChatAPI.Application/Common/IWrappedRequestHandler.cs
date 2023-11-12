using Mediator;

namespace ChatAPI.Application.Common;

public interface IWrappedRequest<T> : IRequest<Result<T>>
    where T : class
{ }

public interface IWrappedRequestHandler<TRequest, TResult> : IRequestHandler<TRequest, Result<TResult>>
    where TResult : class
    where TRequest : IRequest<Result<TResult>>
{ }