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
                lock (_userSyncObj)
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

        private readonly object _userSyncObj = new object();
        private readonly object _deviceSyncObj = new object();
        private readonly object _tokenSyncObj = new object();

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
            if (!IsConnected || _username != username || _password != password)
            {
                lock (_userSyncObj)
                {
                    User = null;
                    _username = username;
                    _password = password;
                }

                try
                {
                    var user = await _connectionHelper.MakeRequestAsync<PlexUser>(Method.Post,
                        PlexResources.MyPlexBaseUrl, PlexResources.MyPlexSignIn, _username, _password);

                    lock (_userSyncObj)
                        User = user;
                }
                catch (Exception ex)
                {
                    throw new NotConnectedToPlexException(ex);
                }
            }

            await RefreshContainerAsync();
        }

        private Guid _refreshToken;

        public async Task RefreshContainerAsync()
        {
            Guid token;

            lock (_tokenSyncObj)
            {
                token = Guid.NewGuid();
                _refreshToken = token;
            }

            try
            {
                var container = await _connectionHelper.MakeRequestAsync<MediaContainer>(Method.Get,
                    PlexResources.MyPlexBaseUrl, PlexResources.MyPlexDevices, user: User);
                
                bool updated;

                lock (_deviceSyncObj)
                {
                    if (token != _refreshToken)
                        return;

                    updated = _devices.UpdateToMatch(container.Devices, d => d.ClientIdentifier, UpdateDevice);
                    _servers.UpdateToMatch(GetByProvides(container, "server"), d => d.ClientIdentifier);
                    _players.UpdateToMatch(GetByProvides(container, "player"), d => d.ClientIdentifier);
                }

                if (updated) OnDevicesUpdated();
            }
            catch
            {
                var updated = false;

                lock (_deviceSyncObj)
                {
                    if (token != _refreshToken)
                        return;

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
            return container.Devices.Where(d => d.Provides.Contains(provides))
                .OrderByDescending(d => d.LastSeenAt).ToList();
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

            List<Device> servers;

            lock (_deviceSyncObj)
                servers = Servers.ToList();

            foreach (var plexServerConnection in servers.Select(s => new PlexServerConnection(_connectionHelper, s, _username, _password)))
            {
                await plexServerConnection.ConnectAsync();
                serverConnections.Add(plexServerConnection);
            }

            return serverConnections;
        }
    }
}