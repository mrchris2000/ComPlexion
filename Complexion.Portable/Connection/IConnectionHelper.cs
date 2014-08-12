using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Complexion.Portable.PlexObjects;

namespace Complexion.Portable.Connection
{
    public interface IConnectionHelper
    {
        [Pure]
        Task<T> MakeRequestAsync<T>(Method method, string baseUrl, string resource = "/",
            string username = null, string password = null, int timeout = 20000, PlexUser user = null)
            where T : class, new();
    }
}