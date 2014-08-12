using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Complexion.Portable.Connection;

namespace Complexion.ios.Connection
{
    public class LocalServerDiscovery : ILocalServerDiscovery
    {
        private const int ReceiveTimeout = 10000;
        private const int SleepTime = 100;

        private UdpClient _udp;
        private readonly HashSet<string> _listeners = new HashSet<string>();
        private bool _listening;
        
        public async Task<IEnumerable<string>> DiscoverLocalServersAsync()
        {
            return await GetListeners();
        }

        private async Task<IEnumerable<string>> GetListeners()
        {
            try
            {
                var result = StartListening();

                var client = new UdpClient();
                var ip = new IPEndPoint(IPAddress.Parse("239.0.0.250"), 32414);
                var bytes = Encoding.ASCII.GetBytes("M-SEARCH * HTTP/1.0");
                client.Send(bytes, bytes.Length, ip);
                client.Close();

                var totalWaitTime = 0;

                while (_listening)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(SleepTime));
                    totalWaitTime += SleepTime;

                    if (totalWaitTime > ReceiveTimeout)
                    {
                        if (!result.IsCompleted)
                        {
                            CleanUp();
                        }
                    }
                }
            }
            catch
            {
                CleanUp();
            }

            return _listeners;
        }

        private void CleanUp()
        {
            try
            {
                _udp.Close();
            }
            catch
            {
            }

            _listening = false;
            _listeners.Clear();
        }

        private IAsyncResult StartListening()
        {
            _listening = true;
            _udp = new UdpClient(32414)
            {
                Client =
                {
                    ReceiveTimeout = ReceiveTimeout,
                    SendTimeout = ReceiveTimeout
                }
            };
            return _udp.BeginReceive(Receive, new object());
        }

        private void Receive(IAsyncResult ar)
        {
            var ip = new IPEndPoint(IPAddress.Any, 32414);

            try
            {
                _udp.EndReceive(ar, ref ip);

                if (_listeners.Add(ip.Address.ToString()))
                    StartListening();
                else
                    _listening = false;
            }
            catch (Exception)
            {
                _listening = false;
            }
        }
    }
}