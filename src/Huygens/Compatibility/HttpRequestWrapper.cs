using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Wrap an ASP.Net request
    /// </summary>
    public class HttpRequestWrapper : IRequest
    {
        /// <inheritdoc />
        public NameValueCollection QueryString { get { return request.QueryString; } }

        /// <inheritdoc />
        public Uri Url { get { return request.Url; } }

        /// <inheritdoc />
        public Stream InputStream { get { return request.InputStream; } }

        /// <inheritdoc />
        public Encoding ContentEncoding { get { return request.ContentEncoding; } }

        /// <inheritdoc />
        public bool HasAcceptEncoding { get { return request.Headers["Accept-Encoding"] != null; } }

        /// <inheritdoc />
        public Version ProtocolVersion { get { return Version.Parse(request.Params["SERVER_PROTOCOL"].SubstringAfter('/')); } }

        /// <inheritdoc />
        public string HttpMethod { get { return request.HttpMethod; } }

        /// <inheritdoc />
        public string RawUrl { get{ return request.RawUrl; } }

        /// <inheritdoc />
        public IHeaderCollection Headers { get { return request.Headers.Wrap(); } }

        /// <inheritdoc />
        public bool IsSecureConnection { get { return request.IsSecureConnection; } }

        /// <inheritdoc />
        public IPEndPoint RemoteEndPoint { get
            {
                var ip = request.UserHostAddress ?? "0.0.0.0";

                if (request.UserHostAddress == "::1")
                {
                    ip = "127.0.0.1";
                }

                return new IPEndPoint(new IPAddress(ip.Split('.').Select(s => Convert.ToByte(s)).ToArray()), 0);
            }
        }

        /// <summary>
        /// Wrapped request
        /// </summary>
        protected HttpRequest request;

        /// <inheritdoc />
        public HttpRequestWrapper(HttpRequest request)
        {
            this.request = request;
        }
    }
}