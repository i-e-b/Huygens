using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Interface for client requests
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Query string components
        /// </summary>
        NameValueCollection QueryString { get; }

        /// <summary>
        /// Request url
        /// </summary>
        Uri Url { get; }

        /// <summary>
        /// Input data if any
        /// </summary>
        Stream InputStream { get; }

        /// <summary>
        /// Encoding of sent data
        /// </summary>
        Encoding ContentEncoding { get; }

        /// <summary>
        /// Client endpoint
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// True if client has provided an accept header
        /// </summary>
        bool HasAcceptEncoding { get; }

        /// <summary>
        /// HTTP version
        /// </summary>
        Version ProtocolVersion { get; }

        /// <summary>
        /// Http request verb
        /// </summary>
        string HttpMethod { get; }

        /// <summary>
        /// Request url as a string
        /// </summary>
        string RawUrl { get; }

        /// <summary>
        /// Request headers
        /// </summary>
        IHeaderCollection Headers { get; }

        /// <summary>
        /// Is this a https connection?
        /// </summary>
        bool IsSecureConnection { get; }
    }
}