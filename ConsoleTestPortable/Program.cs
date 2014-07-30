using System;
using System.Linq;
using Complexion.Portable;
using Complexion.Portable.PlexObjects;

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

            var plex = new MyPlexConnection();
            plex.ConnectAsync(UserName, Password).Wait();

            var servers = plex.Servers.Select(s => new Server(s, UserName, Password)).ToList();

            foreach (var s in servers)
                s.ConnectAsync().Wait();
            
            var server = servers.FirstOrDefault(s => s.IsOnLine);

            if (server != null)
                ShowNowPlaying(server);

            // local connection
            Console.WriteLine();
            Console.WriteLine("Local server connection:");

            var localServer = new Server(LocalServerIp);
            localServer.ConnectAsync().Wait();
            ShowNowPlaying(localServer);

            Console.ReadKey();
        }

        private static void ShowNowPlaying(Server server)
        {
            Console.WriteLine(server.Name);

            var videosTask = server.GetNowPlayingAsync();
            videosTask.Wait();

            var videos = videosTask.Result;
            foreach (var video in videos)
                Console.WriteLine(video.title + " - " + video.ImdbLink);
        }
    }
}
