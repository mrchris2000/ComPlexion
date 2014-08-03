using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Complexion.Portable.Connection;
using Complexion.Portable.Exceptions;

namespace Complexion.Portable.PlexObjects
{
    public class Server
    {
        private readonly Device _device;
        private readonly string _username;
        private readonly string _password;
        private string _connectionUri;

        private MediaContainer _mediaContainer;

        public bool IsOnLine { get { return _mediaContainer != null; } }
        public string Platform { get { return _mediaContainer != null ? _mediaContainer.platform : string.Empty; } }

        public string Name
        {
            get
            {
                if (_mediaContainer != null)
                    return _mediaContainer.friendlyName;
                return _device != null ? _device.name : _connectionUri;
            }
        }

        public Server(Device device, string username, string password)
        {
            _device = device;
            _username = username;
            _password = password;
        }

        public Server(string uri)
        {
            uri = TidyUrl(uri);
            _connectionUri = uri;
        }

        public async Task ConnectAsync()
        {
            if (_device != null)
                await MakeConnectionAsync(_device.Connections, _username, _password);
            else
                await TryConnectionAsync(_connectionUri);
        }

        private static string TidyUrl(string uri)
        {
            if (!uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                uri = "http://" + uri;

            if (uri.IndexOf(':', 5) == -1)
                uri += ":32400";

            return uri;
        }

        private async Task MakeConnectionAsync(IEnumerable<Connection> connections, string username, string password)
        {
            foreach (var connection in connections.Where(c => c.uri != "http://:0"))
            {
                try
                {
                    if (await TryConnectionAsync(connection.uri, username, password))
                    {
                        _connectionUri = connection.uri;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        private async Task<bool> TryConnectionAsync(string uri, string username = null, string password = null)
        {
            var mediaContainer = await ConnectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, uri, "/", username, password);
            if (mediaContainer == null) return false;
            _mediaContainer = mediaContainer;
            return true;
        }

        public async Task<IEnumerable<Video>> GetNowPlayingAsync()
        {
            var container = await ConnectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, _connectionUri, PlexResources.ServerSessions,
                _username, _password);

            if (container == null)
                throw new NotConnectedToPlexException();

            if (container.Videos == null)
                return new List<Video>();

            //foreach (var video in container.Videos)
            //    PreprocessVideo(video);

            return container.Videos;
        }

        private async void PreprocessVideo(Video video)
        {
            video.ArtImage = await ConnectionHelper.MakeRequestAsync<MemoryStream>(Method.Get, _connectionUri,
                video.art, _username, _password);

            video.ThumbImage = await ConnectionHelper.MakeRequestAsync<MemoryStream>(Method.Get, _connectionUri, 
                video.thumb, _username, _password);
        }
    }
}
