using System;
using System.Linq;
using Complexion.Portable;
using Complexion.Win.Connection;

namespace ConsoleTestPortable
{
    static class Program
    {
        private const string UserName = "";
        private const string Password = "";
        private const string LocalServerIp = "";

        static void Main()
        {
            Console.WriteLine("MyPlex connection:");
            var connectionHelper = new ConnectionHelper();

            IMyPlexConnection myPlexConnection = new MyPlexConnection(connectionHelper);
            myPlexConnection.ConnectAsync(UserName, Password).Wait();
            
            var serversTask = myPlexConnection.CreateServerConnectionsAsync();
            serversTask.Wait();
            var servers = serversTask.Result;
            
            var server = servers.FirstOrDefault(s => s.IsOnLine);

            if (server != null)
            {
                Console.WriteLine();
                Console.WriteLine(server.Name);
                ShowNowPlaying(server);
                ShowClients(server);
            }

            // local connection
            Console.WriteLine();
            Console.WriteLine("Local server connection:");

            var localServer = new PlexServerConnection(connectionHelper, LocalServerIp);
            localServer.ConnectAsync().Wait();

            Console.WriteLine(localServer.Name);
            ShowNowPlaying(localServer);
            ShowClients(localServer);

            //localServer.PlayVideo(localServer.NowPlaying[0]).Wait();

            Console.ReadKey();
        }

        private static void ShowClients(IPlexServerConnection server)
        {
            foreach (var client in server.Clients)
                Console.WriteLine("Client - " + client.name);
        }

        private static void ShowNowPlaying(IPlexServerConnection plexServer)
        {
            foreach (var video in plexServer.NowPlaying)
            {
                Console.WriteLine(video.title);
                Console.WriteLine("Playing on " + video.Player.title);
                Console.WriteLine("Links:");
                Console.WriteLine(video.Uri);
                Console.WriteLine(video.SchemeUri);

                Console.WriteLine("Cast:");
                foreach (var role in video.Roles)
                    Console.WriteLine(role.role + ": " + role.tag);

                Console.WriteLine("Directors");
                foreach (var director in video.Directors)
                    Console.WriteLine(director.tag);
            }
        }
    }
}
