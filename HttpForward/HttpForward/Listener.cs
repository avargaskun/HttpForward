using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpForward
{
    public class Listener : IDisposable
    {
        private HttpListener listener;

        private HttpClient client;

        public Listener(string listeningPrefix, string forwardingAddress, bool ignoreSslErrors)
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(listeningPrefix);

            this.client = new HttpClient();
            this.client.BaseAddress = new Uri(forwardingAddress);
            this.client.DefaultRequestHeaders.Add("Headers", $"{this.client.BaseAddress.Host}:{this.client.BaseAddress.Port}");

            if (ignoreSslErrors)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }
        }

        public string Authorization { get; set; }

        public async void StartListening()
        {
            if (this.listener.IsListening)
            {
                throw new NotSupportedException("Listener has already been started.");
            }

            this.listener.Start();

            while (this.listener.IsListening)
            {
                try
                {
                    var context = await this.listener.GetContextAsync();
                    this.HandleRequest(context);
                }
                catch (Exception e)
                {
                    Log.Warning($"Error getting request context {e.GetType().Name}: {e.Message}");
                }
            }
        }

        public void StopListening()
        {
            this.listener.Stop();
        }

        public void Dispose()
        {
            this.listener.Close();
        }

        private async void HandleRequest(HttpListenerContext context)
        {
            HttpRequestMessage request = null;
            HttpResponseMessage response = null;
            try
            {
                request = new HttpRequestMessage();
                request.Method = new HttpMethod(context.Request.HttpMethod);
                request.RequestUri = new Uri(context.Request.Url.PathAndQuery, UriKind.Relative);
                foreach (var header in context.Request.Headers.AllKeys.Where(IsRequestHeader))
                {
                    request.Headers.TryAddWithoutValidation(header, context.Request.Headers[header]);
                }

                if (!request.Headers.Any(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)) && this.Authorization != null)
                {
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Authorization));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64}");
                }

                if (context.Request.ContentLength64 > 0)
                {
                    request.Content = new StreamContent(context.Request.InputStream);
                    foreach (var header in context.Request.Headers.AllKeys.Where(IsContentHeader))
                    {
                        request.Headers.TryAddWithoutValidation(header, context.Request.Headers[header]);
                    }
                }

                response = await this.client.SendAsync(request);

                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.StatusDescription = response.ReasonPhrase;
                foreach (var header in response.Headers)
                {
                    AddResponseHeader(context.Response, header.Key, string.Join(",", header.Value));
                }

                if (response.Content != null)
                {
                    foreach (var header in response.Content.Headers)
                    {
                        AddResponseHeader(context.Response, header.Key, string.Join(",", header.Value));
                    }

                    await (await response.Content.ReadAsStreamAsync()).CopyToAsync(context.Response.OutputStream);
                }

                context.Response.Close();
            }
            catch (Exception e)
            {
                Log.Warning($"Error handling request {e.GetType().Name}: {e.Message}");
                context.Response.Abort();
            }
            finally
            {
                if (request != null)
                {
                    if (request.Content != null)
                    {
                        request.Content.Dispose();
                    }

                    request.Dispose();
                }

                if (response != null)
                {
                    if (response.Content != null)
                    {
                        response.Content.Dispose();
                    }

                    response.Dispose();
                }
            }
        }

        private static readonly string[] ContentHeaders =
        {
            "Allow",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            "Content-Type",
            "Expires",
            "Last-Modified"
        };

        private static readonly string[] SpecialHeaders =
        {
            "Host",
            "WWW-Authenticate",
            "Transfer-Encoding",
            "Keep-Alive"
        };

        private static bool IsRequestHeader(string header)
        {
            return !IsContentHeader(header) && !SpecialHeaders.Contains(header, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsContentHeader(string header)
        {
            return ContentHeaders.Contains(header, StringComparer.OrdinalIgnoreCase);
        }

        private static void AddResponseHeader(HttpListenerResponse response, string header, string value)
        {
            if (header.Equals("Content-Length"))
            {
                response.ContentLength64 = long.Parse(value);
            }
            else if (!SpecialHeaders.Contains(header, StringComparer.OrdinalIgnoreCase))
            {
                response.AddHeader(header, value);
            }
        }
    }
}
