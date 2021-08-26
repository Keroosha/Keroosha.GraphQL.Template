using System.Threading.Tasks;

namespace Keroosha.GraphQL.Web.Cmdlets
{
    public interface ICmdletExec
    {
        Task<int> Execute(object args);
    }

    public abstract class CmdletBase<T> : ICmdletExec
    {
        public Task<int> Execute(object args) => Execute((T)args);

        protected abstract Task<int> Execute(T args);
    }
}