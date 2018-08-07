using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Helpers for converting from Serialisable to Contextual requests and responses
    /// </summary>
    public static class HttpConverters {
        /// <summary>
        /// Convert a ASP.Net or HTTP Listener request to a serialisable one
        /// </summary>
        public static SerialisableRequest ConvertToSerialisable(IRequest request) {
            var newRq = new SerialisableRequest
            {
                IsSecureConnection = request.IsSecureConnection,
                Content = GetBytes(request.InputStream),
                Method = request.HttpMethod,
                RequestUri = request.Url.PathAndQuery,
                Headers = MapHeadersToDict(request.Headers)
            };
            return newRq;
        }

        /// <summary>
        /// Copy from a WebResponse to a HttpReponse
        /// </summary>
        public static void CopyTo(this HttpWebResponse src, HttpResponse dst)
        {
            if (src == null) {
                throw new ArgumentNullException(nameof(src));
            }
            dst.StatusDescription = src.StatusDescription ?? (src.StatusCode.ToString());
            dst.ContentType = src.ContentType;
            dst.StatusCode = (int)src.StatusCode;

            // Copy unrestricted headers (including cookies, if any)
            foreach (var headerKey in src.Headers.AllKeys)
            {
                switch (headerKey)
                {
                    case "Connection":
                    case "Content-Length":
                    case "Transfer-Encoding":
                        // Let IIS handle these
                        break;

                    case "Content-Type":
                    case "Referer":
                    case "User-Agent":
                        break;

                    default:
                        dst.Headers[headerKey] = src.Headers[headerKey];
                        break;
                }
            }
            
            var body = src.GetResponseStream();
            if (body != null) body.CopyTo(dst.OutputStream);
        }

        /// <summary>
        /// Copies all headers and content (except the URL) from an incoming to an outgoing
        /// request.
        /// </summary>
        public static void CopyTo(this HttpRequest src, HttpWebRequest dst)
        {
            dst.Method = src.HttpMethod;

            // Copy unrestricted headers (including cookies, if any)
            foreach (var headerKey in src.Headers.AllKeys)
            {
                switch (headerKey)
                {
                    case "Connection":
                    case "Content-Length":
                    case "Date":
                    case "Expect":
                    case "Host":
                    case "If-Modified-Since":
                    case "Range":
                    case "Transfer-Encoding":
                    case "Proxy-Connection":
                        // Let IIS handle these
                        break;

                    case "Accept":
                    case "Content-Type":
                    case "Referer":
                    case "User-Agent":
                        // Restricted - copied below
                        break;

                    default:
                        dst.Headers[headerKey] = src.Headers[headerKey];
                        break;
                }
            }

            // Copy restricted headers
            if (src.AcceptTypes != null && src.AcceptTypes.Any())
            {
                dst.Accept = string.Join(",", src.AcceptTypes);
            }
            dst.ContentType = src.ContentType;
            dst.Referer = src.UrlReferrer?.AbsoluteUri;
            dst.UserAgent = src.UserAgent;

            if (src.HttpMethod != "GET" && src.HttpMethod != "HEAD" && src.ContentLength > 0)
            {
                var destinationStream = dst.GetRequestStream();
                src.InputStream.CopyTo(destinationStream);
                destinationStream.Close();
            }
        }
        
        private static Dictionary<string,string> MapHeadersToDict(IHeaderCollection requestHeaders)
        {
            var heads = new Dictionary<string,string>();
            foreach (var key in requestHeaders.AllKeys)  {
                heads.Add(key, requestHeaders[key]);
            }
            return heads;
        }

        private static byte[] GetBytes(Stream stream)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Copy a serialisable response to a contextual response.
        /// Any redirect to a 0.0.0.0 IP will be replaced with the rebaseUrl
        /// </summary>
        public static void CopyToHttpListener(SerialisableResponse result, IResponse response, string rebaseUrl)
        {
            response.StatusCode = result.StatusCode;

            CopyHeadersToHttpListener(result, response.Headers, rebaseUrl);
            response.StatusDescription = result.StatusMessage;

            // output stream must be last
            if (result.Content != null) response.OutputStream.Write(result.Content, 0, result.Content.Length);
        }

        private static void CopyHeadersToHttpListener(SerialisableResponse response, IHeaderCollection responseHeaders, string rebaseEndpoint)
        {
            if (response == null || response.Headers == null) return;

            response.Headers.Remove("Pragma");
            response.Headers.Add("Server", new[] { string.Empty });
            response.Headers.Remove("X-Powered-By");

            foreach (var pair in response.Headers)
            {
                // Header filters:
                switch (pair.Key)
                {
                    // Skip headers we MUST NOT change:
                    case "Transfer-Encoding": // prevent mismatch of Chunked encoding from breaking things
                    case "Content-Length": // we handle this one specially
                        continue;
                    
                    // Filter out vanity headers:
                    case "Pragma":
                    case "Server":
                    case "X-Powered-By":
                    case "Via":
                        continue;
                }

                if (pair.Key == "Location")
                {
                    if (Uri.TryCreate(string.Join(",", pair.Value), UriKind.Absolute, out var redirectUrl))
                    {
                        if (redirectUrl.Host == "0.0.0.0") {// local redirect. Pass back the *external* url
                            var newTarget = rebaseEndpoint + redirectUrl.PathAndQuery;
                            responseHeaders[pair.Key] = newTarget;
                            continue;
                        }
                    }
                }

                if (responseHeaders[pair.Key] == null)
                {
                    responseHeaders[pair.Key] = string.Join(",", pair.Value);
                }
                else
                {
                    responseHeaders.Add(pair.Key, string.Join(", ", pair.Value));
                }
            }
        }

        /// <summary>
        /// Send a local file back through a context
        /// </summary>
        public static void SendFile(string filePath, IContext context)
        {
            var response = context.Response;
            var mapped = context.MapPath(filePath);
            if (! File.Exists(mapped)) {
                response.StatusCode = 404;
                response.StatusDescription = "Not Found";
                return;
            }

            var info = new FileInfo(mapped);
            try { response.ContentLength64 = info.Length; }
            catch { Ignore(); }
            try
            {
                response.StatusCode = 200;
                response.StatusDescription = "OK";
            }
            catch { Ignore(); }
            try { response.ContentType = GuessMime(mapped); }
            catch { Ignore(); }

            using (var fs = File.OpenRead(mapped))
            {
                fs.CopyTo(response.OutputStream);
            }
        }

        private static void Ignore() { }

        private static string GuessMime(string filePath)
        {
            return Internal.NetworkUtils.GetContentType(filePath);
        }
    }
}