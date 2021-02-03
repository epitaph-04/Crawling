  using MediatR;

namespace Infrastructure
{
	public interface IQuery<out TResponse> : IRequest<TResponse> { }
}
