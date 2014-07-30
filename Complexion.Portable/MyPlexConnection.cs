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
    public class MyPlexConnection : IMyPlexConnection
    {
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
        public ReadOnlyObservableCollection<Device> Devices { get; private set; }
        
        private readonly ObservableCollectionEx<Device> _servers;
        public ReadOnlyObservableCollection<Device> Servers { get; private set; }

        private readonly ObservableCollectionEx<Device> _players;
        public ReadOnlyObservableCollection<Device> Players { get; private set; }

        private string _username;
        private string _password;

        private readonly object _syncObj = new object();

        public MyPlexConnection()
        {
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
                var user = await ConnectionHelper.MakeRequestAsync<PlexUser>(Method.Post, 
                    PlexResources.MyPlexBaseUrl, PlexResources.MyPlexSignIn, _username, _password);
                var container = await ConnectionHelper.MakeRequestAsync<MediaContainer>(Method.Get, 
                    PlexResources.MyPlexBaseUrl, PlexResources.MyPlexDevices, _username, _password);

                lock (_syncObj)
                {
                    User = user;

                    _devices.ClearAndAddRange(container.Devices);
                    _servers.ClearAndAddRange(GetByProvides(container, "server"));
                    _players.ClearAndAddRange(GetByProvides(container, "player"));
                }

            }
            catch (Exception ex)
            {
                throw new NotConnectedToPlexException(ex);
            }

        }

        private static IEnumerable<Device> GetByProvides(MediaContainer container, string provides)
        {
            return container.Devices.Where(d => d.provides.Contains(provides)).OrderByDescending(d => d.lastSeenAt);
        }
    }
}