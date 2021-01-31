  using MediatR;

namespace Api.Infrastructure
{
	public interface IQuery<out TResponse> : IRequest<TResponse> { }
}
