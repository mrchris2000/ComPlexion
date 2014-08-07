using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Complexion.Portable.Connection;
using Complexion.Portable.Exceptions;
using Complexion.Portable.PlexObjects;

namespace Complexion.Portable
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class MyPlexConnection : IMyPlexConnection
    {
        private readonly IConnectionHelper _connectionHelper;

        #region User

        private PlexUser _user;

        public PlexUser User
        {
            get
            {
                lock (_syncObj)
                {
                    if (_user == null)
                        throw new NotConnectedToPlexException();

                    return _user;
                }
            }
            private set { _user = value; }
        }

        #endregion User

        private readonly ObservableCollectionEx<Device> _devices;
        private readonly ObservableCollectionEx<Device> _servers;
        private readonly ObservableCollectionEx<Device> _players;

        public ReadOnlyObservableCollection<Device> Devices { get; private set; }
        public ReadOnlyObservableCollection<Device> Servers { get; private set; }
        public ReadOnlyObservableCollection<Device> Players { get; private set; }

        private string _username;
        private string _password;

        private readonly object _syncObj = new object();

        public MyPlexConnection(IConnectionHelper connectionHelper)
        {
            _connectionHelper = connectionHelper;
            _devices = new ObservableCollectionEx<Device>();
            _servers = new ObservableCollectionEx<Device>();
            _players = new ObservableCollectionEx<Device>();

            Devices = new ReadOnlyObservableCollection<Device>(_devices);
            Servers = new ReadOnlyObservableCollection<Device>(_servers);
            Players = new ReadOnlyObservableCollection<Device>(_players);
        }

        public async Task ConnectAsync(string username, string password)
        {
            lock (_syncObj)
            {
                User = null;
                _username = username;
                _password = password;
            }

            try
            {
                var user = await _connectionHelper.MakeRequestAsync<PlexUser>(Method.Post, 
                    PlexResources.MyPlexBaseUrl, PlexResources.MyPlexSignIn, _username, _password);

                lock (_syncObj)
                    User = user;
            }
            catch (Exception ex)
            {
                throw new NotConnectedToPlexException(ex);
            }

            await RefreshContainer();
        }

        public async Task RefreshContainer()
        {
            try
            {
                var container = await _connectionHelper.MakeRequestAsync<MediaContainer>(Method.Get,
                    PlexResources.MyPlexBaseUrl, PlexResources.MyPlexDevices, _username, _password);

                bool updated;

                lock (_syncObj)
                {
                    updated = _devices.UpdateToMatch(container.Devices, d => d.clientIdentifier, UpdateDevice);
                    _servers.UpdateToMatch(GetByProvides(container, "server"), d => d.clientIdentifier);
                    _players.UpdateToMatch(GetByProvides(container, "player"), d => d.clientIdentifier);
                }

                if (updated) OnDevicesUpdated();
            }
            catch
            {
                var updated = false;

                lock (_syncObj)
                {
                    // lost connection, so clear everything
                    if (_devices.Any())
                    {
                        _devices.Clear();
                        _servers.Clear();
                        _players.Clear();

                        updated = true;
                    }
                }

                if (updated) OnDevicesUpdated();
            }
        }

        private static bool UpdateDevice(Device oldDevice, Device newDevice)
        {
            return oldDevice.UpdateFrom(newDevice);
        }

        private static ICollection<Device> GetByProvides(MediaContainer container, string provides)
        {
            return container.Devices.Where(d => d.provides.Contains(provides))
                .OrderByDescending(d => d.lastSeenAt).ToList();
        }

        public bool IsConnected { get { return _user != null; } }

        public event EventHandler DevicesUpdated;

        private void OnDevicesUpdated()
        {
            var handler = DevicesUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public async Task<IEnumerable<IPlexServerConnection>> CreateServerConnectionsAsync()
        {
            var serverConnections = new List<IPlexServerConnection>();
            var servers = Servers.ToList();

            foreach (var plexServerConnection in servers.Select(s => new PlexServerConnection(_connectionHelper, s, _username, _password)))
            {
                await plexServerConnection.ConnectAsync();
                serverConnections.Add(plexServerConnection);
            }

            return serverConnections;
        }
    }
}