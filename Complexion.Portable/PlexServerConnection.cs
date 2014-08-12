using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Complexion.Portable.Connection;
using Complexion.Portable.Exceptions;
using Complexion.Portable.PlexObjects;
using JimBobBennett.JimLib.Collections;
using JimBobBennett.JimLib.Mvvm;

namespace Complexion.Portable
{
    public class PlexServerConnection : NotificationObject, IPlexServerConnection
    {
        private readonly IConnectionHelper _connectionHelper;
        private Device _device;
        private string _username;
        private string _password;
        private string _connectionUri;
        private MediaContainer _mediaContainer;

        [NotifyPropertyChangeDependency("Name")]
        public Device Device
        {
            get { return _device; }
            private set
            {
                if (Equals(Device, value)) return;
                _device = value;
                RaisePropertyChanged();
            }
        }

        public string Username
        {
            get { return _username; }
            private set
            {
                if (Equals(Username, value)) return;
                _username = value;
                RaisePropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            private set
            {
                if (Equals(Password, value)) return;
                _password = value;
                RaisePropertyChanged();
            }
        }

        [NotifyPropertyChangeDependency("Name")]
        public string ConnectionUri
        {
            get { return _connectionUri; }
            private set
            {
                if (Equals(ConnectionUri, value)) return;
                _connectionUri = value;
                RaisePropertyChanged();
            }
        }

        [NotifyPropertyChangeDependency("IsOnLine")]
        [NotifyPropertyChangeDependency("Platform")]
        [NotifyPropertyChangeDependency("MachineIdentifier")]
        [NotifyPropertyChangeDependency("Name")]
        public MediaContainer MediaContainer
        {
            get { return _mediaContainer; }
            private set
            {
                if (Equals(MediaContainer, value)) return;
                _mediaContainer = value;
                RaisePropertyChanged();
            }
        }

        public bool IsOnLine { get { return MediaContainer != null; } }
        public string Platform { get { return MediaContainer != null ? MediaContainer.Platform : string.Empty; } }
        public string MachineIdentifier { get { return MediaContainer != null ? MediaContainer.MachineIdentifier : string.Empty; } }

        public string Name
        {
            get
            {
                if (MediaContainer != null)
                    return MediaContainer.FriendlyName;

                return Device != null ? Device.Name : ConnectionUri;
            }
        }
        
        private readonly ObservableCollectionEx<Video> _nowPlaying = new ObservableCollectionEx<Video>();
        private readonly ObservableCollectionEx<Server> _clients = new ObservableCollectionEx<Server>();

        public ReadOnlyObservableCollection<Video> NowPlaying { get; private set; }
        public ReadOnlyObservableCollection<Server> Clients { get; private set; }

        private PlexServerConnection(IConnectionHelper connectionHelper)
        {
            _connectionHelper = connectionHelper;
            NowPlaying = new ReadOnlyObservableCollection<Video>(_nowPlaying);
            Clients = new ReadOnlyObservableCollection<Server>(_clients);
        }

        public PlexServerConnection(IConnectionHelper connectionHelper, Device device, string username, string password)
            : this(connectionHelper)
        {
            Device = device;
            Username = username;
            Password = password;
        }

        public PlexServerConnection(IConnectionHelper connectionHelper, string uri, string username = null, string password = null)
            : this(connectionHelper)
        {
            ConnectionUri = TidyUrl(uri);
            Username = username;
            Password = password;
        }

        public async Task ConnectAsync()
        {
            if (Device != null)
                await MakeConnectionAsync(Device.Connections);
            else
                await TryConnectionAsync(ConnectionUri);

            if (IsOnLine)
                await RefreshSessionAsync();
        }

        private static string TidyUrl(string uri)
        {
            if (!uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                uri = "http://" + uri;

            if (uri.IndexOf(':', 5) == -1)
                uri += ":32400";

            return uri;
        }

        private async Task MakeConnectionAsync(IEnumerable<PlexObjects.Connection> connections)
        {
            foreach (var connection in connections.Where(c => c.Uri != "http://:0"))
            {
                try
                {
                    if (await TryConnectionAsync(connection.Uri))
                    {
                        ConnectionUri = connection.Uri;
                        return;
                    }
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                    
                }
            }

            ClearMediaContainer();
        }

        private void ClearMediaContainer()
        {
            MediaContainer = null;
            _nowPlaying.Clear();
            _clients.Clear();
        }

        private async Task<bool> TryConnectionAsync(string uri)
        {
            var mediaContainer = await _connectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, uri, "/", 
                Username, Password);

            if (mediaContainer == null) return false;

            if (MediaContainer == null)
            {
                MediaContainer = mediaContainer;
                await RefreshSessionAsync();
            }
            else
            {
                if (MediaContainer.UpdateFrom(mediaContainer))
                    RaisePropertyChanged(() => MediaContainer);

                await RefreshSessionAsync();
            }

            return true;
        }

        public async Task RefreshAsync()
        {
            var connected = await TryConnectionAsync(ConnectionUri);

            if (connected)
                await RefreshSessionAsync();
            else
                ClearMediaContainer();
        }

        private async Task RefreshSessionAsync()
        {
            IList<Video> videos;
            IList<Server> clients;

            try
            {
                videos = await GetNowPlayingAsync();
            }
            catch
            {
                videos = new List<Video>();
            }

            try
            {
                clients = await GetClientsAsync();
            }
            catch
            {
                clients = new List<Server>();
            }

            foreach (var video in videos)
            {
                var client = clients.FirstOrDefault(c => c.Key == video.Player.Key);
                if (client != null)
                    video.Player.Client = client;
            }

            _clients.UpdateToMatch(clients, v => v.Key, UpdateClient);
            _nowPlaying.UpdateToMatch(videos, v => v.Key, UpdateVideo);
        }

        private static bool UpdateVideo(Video oldVideo, Video newVideo)
        {
            return oldVideo.UpdateFrom(newVideo);
        }

        private static bool UpdateClient(Server oldClient, Server newClient)
        {
            return oldClient.UpdateFrom(newClient);
        }

        private async Task<IList<Video>> GetNowPlayingAsync()
        {
            var container = await _connectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, ConnectionUri, 
                PlexResources.ServerSessions,Username, Password);

            if (container == null)
                throw new NotConnectedToPlexException();

            if (container.Videos == null)
                return new List<Video>();

            foreach (var video in container.Videos)
            {
                video.Art = ConnectionUri + video.Art;
                video.Thumb = ConnectionUri + video.Thumb;
            }

            return container.Videos;
        }

        private async Task<IList<Server>> GetClientsAsync()
        {
            var container = await _connectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, ConnectionUri, 
                PlexResources.ServerClients, Username, Password);

            if (container == null)
                throw new NotConnectedToPlexException();

            return container.Servers ?? new ObservableCollectionEx<Server>();
        }

        public async Task PauseVideoAsync(Video video)
        {
            await ChangeClientPlayback(video, PlexResources.ClientPause);
        }

        public async Task PlayVideoAsync(Video video)
        {
            await ChangeClientPlayback(video, PlexResources.ClientPlay);
        }

        public async Task StopVideoAsync(Video video)
        {
            await ChangeClientPlayback(video, PlexResources.ClientStop);
        }

        private async Task ChangeClientPlayback(Video video, string action)
        {
            if (video != null && video.Player != null && video.Player.Client != null)
            {
                var client = video.Player.Client;

                var clientUriBuilder = new UriBuilder
                {
                    Port = client.Port,
                    Host = client.Host,
                    Scheme = "http"
                };

                await _connectionHelper.MakeRequestAsync<Response>(Method.Get, 
                    clientUriBuilder.Uri.ToString(), action, Username, Password);
            }
        }
    }
}
