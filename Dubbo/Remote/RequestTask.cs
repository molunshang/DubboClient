using System.Threading.Tasks;

namespace Dubbo.Remote
{
    public class RequestTask
    {
        public RequestTask(Request request, TaskCompletionSource<Response> task)
        {
            Request = request;
            Task = task;
        }
        public Request Request { get; private set; }
        public TaskCompletionSource<Response> Task { get; private set; }
    }
}