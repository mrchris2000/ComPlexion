using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Complexion.Portable.PlexObjects;

namespace Complexion.Portable
{
    public interface IMyPlexConnection
    {
        PlexUser User { get; }
        ReadOnlyObservableCollection<Device> Devices { get; }
        ReadOnlyObservableCollection<Device> Servers { get; }
        ReadOnlyObservableCollection<Device> Players { get; }
        bool IsConnected { get; }
        Task ConnectAsync(string username, string password);
        Task RefreshContainer();
        event EventHandler DevicesUpdated;
        Task<IEnumerable<IPlexServerConnection>> CreateServerConnectionsAsync();
    }
}