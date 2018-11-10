using System.Threading;
using System.Threading.Tasks;

namespace SportApi.Scheduler
{
	public interface IScheduledTask
	{
		string Schedule { get; }
		Task ExecuteAsync(CancellationToken cancellationToken);
	}
}
