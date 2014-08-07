using System.Threading.Tasks;

namespace Complexion.Portable.Connection
{
    public interface IConnectionHelper
    {
        Task<T> MakeRequestAsync<T>(Method method, string baseUrl, string resource = "/",
            string username = null, string password = null, int timeout = 20000)
            where T : class, new();
    }
}