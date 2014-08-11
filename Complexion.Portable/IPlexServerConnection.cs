using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Complexion.Portable.PlexObjects;

namespace Complexion.Portable
{
    public interface IPlexServerConnection : INotifyPropertyChanged
    {
        Device Device { get; }
        string Username { get; }
        string Password { get; }
        string ConnectionUri { get; }
        bool IsOnLine { get; }
        string Platform { get; }
        string MachineIdentifier { get; }
        string Name { get; }

        ReadOnlyObservableCollection<Video> NowPlaying { get; }
        ReadOnlyObservableCollection<Server> Clients { get; }

        Task ConnectAsync();
        Task RefreshAsync();
        Task PauseVideoAsync(Video video);
        Task PlayVideoAsync(Video video);
        Task StopVideoAsync(Video video);
    }
}