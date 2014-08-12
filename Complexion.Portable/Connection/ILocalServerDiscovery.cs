using System.Collections.Generic;
using System.Threading.Tasks;

namespace Complexion.Portable.Connection
{
    public interface ILocalServerDiscovery
    {
        Task<IEnumerable<string>> DiscoverLocalServersAsync();
    }
}
