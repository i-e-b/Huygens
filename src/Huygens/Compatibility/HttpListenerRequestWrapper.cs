using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Request wrapper for .Net http listener
    /// </summary>
    public class HttpListenerRequestWrapper : IRequest
    {
        /// <inheritdoc />
        public NameValueCollection QueryString { get { return request.QueryString; } }

        /// <inheritdoc />
        public Uri Url {get { return request.Url; } }

        /// <inheritdoc />
        public Stream InputStream { get { return request.InputStream; } }

        /// <inheritdoc />
        public Encoding ContentEncoding { get { return request.ContentEncoding; } }

        /// <inheritdoc />
        public IPEndPoint RemoteEndPoint { get { return request.RemoteEndPoint; } }

        /// <inheritdoc />
        public bool HasAcceptEncoding { get { return request.Headers["Accept-Encoding"] != null; } }

        /// <inheritdoc />
        public Version ProtocolVersion { get{ return request.ProtocolVersion;} }

        /// <inheritdoc />
        public string HttpMethod { get{ return request.HttpMethod;} }

        /// <inheritdoc />
        public string RawUrl { get{ return request.RawUrl;} }

        /// <inheritdoc />
        public IHeaderCollection Headers { get{ return request.Headers.Wrap(); } }

        /// <inheritdoc />
        public bool IsSecureConnection { get { return request.IsSecureConnection; } }

        /// <summary>
        /// wrapped request
        /// </summary>
        protected HttpListenerRequest request;

        /// <summary>
        /// Wrap a request
        /// </summary>
        public HttpListenerRequestWrapper(HttpListenerRequest request)
        {
            this.request = request;
        }
    }
}