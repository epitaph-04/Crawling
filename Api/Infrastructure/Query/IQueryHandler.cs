using MediatR;

namespace Api.Infrastructure
{
    public interface IQueryHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IQuery<TResponse> { }
}