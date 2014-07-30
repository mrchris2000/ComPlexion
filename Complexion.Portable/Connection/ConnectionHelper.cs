using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Complexion.Portable.Connection
{
    internal enum Method
    {
        Get,
        Post,
    }

    internal static class ConnectionHelper
    {
        private static void CreatePlexRequest(HttpHeaders headers)
        {
            headers.Add("X-Plex-Platform", "Windows");
            headers.Add("X-Plex-Platform-Version", "NT");
            headers.Add("X-Plex-Provides", "player");
            headers.Add("X-Plex-Client-Identifier", "Complexion");
            headers.Add("X-Plex-Product", "PlexWMC");
            headers.Add("X-Plex-Version", "0");
        }

        internal static async Task<T> MakeRequestAsync<T>(Method method, string baseUrl, string resource = "/",
            string username = null, string password = null, int timeout = 20000)
            where T : class, new()
        {
            using (var clientHandler = new HttpClientHandler{UseCookies = false})
            {
                if (!string.IsNullOrEmpty(username))
                    clientHandler.Credentials = new NetworkCredential(username, password);

                using (var client = new HttpClient(clientHandler))
                {
                    try
                    {
                        client.BaseAddress = new Uri(baseUrl);
                        client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                        
                        CreatePlexRequest(client.DefaultRequestHeaders);

                        var requestUri = new Uri(resource, UriKind.Relative);

                        switch (method)
                        {
                            case Method.Get:
                                var responseBody = await client.GetStringAsync(requestUri);
                                if (string.IsNullOrEmpty(responseBody))
                                    return null;;
                                return DeserializeResponse<T>(responseBody);

                            case Method.Post:
                                var response = await client.PostAsync(requestUri, new StringContent(string.Empty));
                                response.EnsureSuccessStatusCode();
                                var postResponseBody = await response.Content.ReadAsStringAsync();
                                if (string.IsNullOrEmpty(postResponseBody))
                                    return null;;
                                return DeserializeResponse<T>(postResponseBody);
                        }

                        return null;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
        }

        private static T DeserializeResponse<T>(string responseBody)
        {
            return new XmlDeserializer().Deserialize<T>(responseBody);
        }
    }
}
