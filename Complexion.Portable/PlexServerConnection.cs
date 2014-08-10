using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Complexion.Portable.Connection;
using Complexion.Portable.Exceptions;
using Complexion.Portable.PlexObjects;
using JimBobBennett.JimLib.Collections;

namespace Complexion.Portable
{
    public class PlexServerConnection : IPlexServerConnection
    {
        private readonly IConnectionHelper _connectionHelper;

        public Device Device { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string ConnectionUri { get; private set; }

        private readonly ObservableCollectionEx<Video> _nowPlaying = new ObservableCollectionEx<Video>();
        private readonly ObservableCollectionEx<Server> _clients = new ObservableCollectionEx<Server>();

        public ReadOnlyObservableCollection<Video> NowPlaying { get; private set; }
        public ReadOnlyObservableCollection<Server> Clients { get; private set; } 
        
        private MediaContainer _mediaContainer;

        public bool IsOnLine { get { return _mediaContainer != null; } }
        public string Platform { get { return _mediaContainer != null ? _mediaContainer.platform : string.Empty; } }
        public string MachineIdentifier { get { return _mediaContainer != null ? _mediaContainer.machineIdentifier : string.Empty; } }

        public string Name
        {
            get
            {
                if (_mediaContainer != null)
                    return _mediaContainer.friendlyName;

                return Device != null ? Device.name : ConnectionUri;
            }
        }

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
                await MakeConnectionAsync(Device.Connections, Username, Password);
            else
                await TryConnectionAsync(ConnectionUri);

            if (IsOnLine)
                await RefreshAsync();
        }

        private static string TidyUrl(string uri)
        {
            if (!uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                uri = "http://" + uri;

            if (uri.IndexOf(':', 5) == -1)
                uri += ":32400";

            return uri;
        }

        private async Task MakeConnectionAsync(IEnumerable<PlexObjects.Connection> connections, string username, 
            string password)
        {
            foreach (var connection in connections.Where(c => c.uri != "http://:0"))
            {
                try
                {
                    if (await TryConnectionAsync(connection.uri, username, password))
                    {
                        ConnectionUri = connection.uri;
                        return;
                    }
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                    
                }
            }

            _mediaContainer = null;
        }

        private async Task<bool> TryConnectionAsync(string uri, string username = null, string password = null)
        {
            var mediaContainer = await _connectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, uri, "/", 
                username, password);
            if (mediaContainer == null) return false;
            _mediaContainer = mediaContainer;
            return true;
        }

        public async Task RefreshAsync()
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
                video.art = ConnectionUri + video.art;
                video.thumb = ConnectionUri + video.thumb;
            }

            return container.Videos;
        }

        private async Task<IList<Server>> GetClientsAsync()
        {
            var container = await _connectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, ConnectionUri, 
                PlexResources.ServerClients, Username, Password);

            if (container == null)
                throw new NotConnectedToPlexException();

            return container.Servers ?? new List<Server>();
        }

        public async Task PauseVideoAsync(Video video)
        {
            if (video != null && video.Player != null && video.Player.Client != null)
            {
                var client = video.Player.Client;

                var clientUriBuilder = new UriBuilder
                {
                    Port = client.port,
                    Host = client.host,
                    Scheme = "http"
                };

                await _connectionHelper.MakeRequestAsync<Response>(Method.Get, clientUriBuilder.Uri.ToString(),
                    PlexResources.ClientPause, Username, Password);
            }
        }

        public async Task PlayVideoAsync(Video video)
        {
            if (video != null && video.Player != null && video.Player.Client != null)
            {
                var client = video.Player.Client;

                var clientUriBuilder = new UriBuilder
                {
                    Port = client.port,
                    Host = client.host,
                    Scheme = "http"
                };

                await _connectionHelper.MakeRequestAsync<Response>(Method.Get, clientUriBuilder.Uri.ToString(),
                    PlexResources.ClientPlay, Username, Password);
            }
        }
    }
}
