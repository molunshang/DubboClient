using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Dubbo.Remote
{
    public class RequestTasks
    {
        private static ConcurrentDictionary<long, RequestTask> waitingTasks = new ConcurrentDictionary<long, RequestTask>();
        public static Task<Response> NewRequest(Request request)
        {
            var taskSource = new TaskCompletionSource<Response>();
            waitingTasks.TryAdd(request.RequestId, new RequestTask(request, taskSource));
            return taskSource.Task;
        }

        public static RequestTask GetRequestTask(long id)
        {
            return waitingTasks.TryRemove(id, out var task) ? task : null;
        }
    }
}