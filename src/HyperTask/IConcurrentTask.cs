using System.Threading;
using System.Threading.Tasks;

namespace HyperTask
{
    public interface IConcurrentTask
    {
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}
