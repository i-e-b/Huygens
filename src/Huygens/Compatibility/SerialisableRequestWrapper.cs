using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Huygens.Compatibility
{
    /// <summary>
    /// IRequest wrapper for SerialisableRequest objects
    /// </summary>
    public class SerialisableRequestWrapper : IRequest
    {
        private readonly SerialisableRequest _request;

        /// <summary>
        /// Wrap a request object
        /// </summary>
        public SerialisableRequestWrapper(SerialisableRequest request)
        {
            _request = request;
            Headers = new DictionaryHeaderWrapper(request.Headers);
        }

        /// <inheritdoc />
        public NameValueCollection QueryString { get { return Url.Query.ParseQueryString(); } }

        /// <inheritdoc />
        public Uri Url => new Uri(_request.RequestUri, UriKind.Absolute);

        /// <inheritdoc />
        public Stream InputStream
        {
            get
            {
                if (_request.Content == null) return null;
                return new MemoryStream(_request.Content, false);
            }
        }

        /// <inheritdoc />
        public Encoding ContentEncoding
        {
            get
            {
                if (!_request.Headers.TryGetValue("Content-Type", out var contentType)) return Encoding.UTF8;
                var attr = GetAttributeFromHeader(contentType, "charset");
                if (string.IsNullOrWhiteSpace(attr)) return Encoding.UTF8;
                return Encoding.GetEncoding(attr);
            }
        }


        /// <summary>
        /// Client endpoint. Returns a dummy value in this implementation
        /// </summary>
        public IPEndPoint RemoteEndPoint { get { return new IPEndPoint(0L, 0); } }

        /// <inheritdoc />
        public bool HasAcceptEncoding { get { return Headers["Accept-Encoding"] != null; } }

        /// <summary>
        /// HTTP version. Always returns 1.1
        /// </summary>
        public Version ProtocolVersion { get; } = new Version(1, 1);

        /// <inheritdoc />
        public string HttpMethod { get { return _request.Method; } }

        /// <inheritdoc />
        public string RawUrl { get { return _request.RequestUri; } }

        /// <inheritdoc />
        public IHeaderCollection Headers { get; }

        /// <inheritdoc />
        public bool IsSecureConnection { get { return _request.IsSecureConnection; } }


        // from System.Net.HttpListenerRequest.Helpers
        internal static string GetAttributeFromHeader(string headerValue, string attrName)
        {
            if (headerValue == null)
                return null;
            int length1 = headerValue.Length;
            int length2 = attrName.Length;
            int startIndex1 = 1;
            while (startIndex1 < length1)
            {
                startIndex1 = CultureInfo.InvariantCulture.CompareInfo.IndexOf(headerValue, attrName, startIndex1, CompareOptions.IgnoreCase);
                if (startIndex1 >= 0 && startIndex1 + length2 < length1)
                {
                    char c1 = headerValue[startIndex1 - 1];
                    char c2 = headerValue[startIndex1 + length2];
                    if (c1 != 59 && c1 != 44 && !char.IsWhiteSpace(c1) || c2 != 61 && !char.IsWhiteSpace(c2))
                        startIndex1 += length2;
                    else
                        break;
                }
                else
                    break;
            }
            if (startIndex1 < 0 || startIndex1 >= length1)
                return null;
            int index1 = startIndex1 + length2;
            while (index1 < length1 && char.IsWhiteSpace(headerValue[index1]))
                ++index1;
            if (index1 >= length1 || headerValue[index1] != 61)
                return null;
            int startIndex2 = index1 + 1;
            while (startIndex2 < length1 && char.IsWhiteSpace(headerValue[startIndex2]))
                ++startIndex2;
            if (startIndex2 >= length1)
                return null;
            string str;
            if (startIndex2 < length1 && headerValue[startIndex2] == 34)
            {
                if (startIndex2 == length1 - 1)
                    return null;
                int num = headerValue.IndexOf('"', startIndex2 + 1);
                if (num < 0 || num == startIndex2 + 1)
                    return null;
                str = headerValue.Substring(startIndex2 + 1, num - startIndex2 - 1).Trim();
            }
            else
            {
                int index2 = startIndex2;
                while (index2 < length1 && (headerValue[index2] != 32 && headerValue[index2] != 44))
                    ++index2;
                if (index2 == startIndex2)
                    return null;
                str = headerValue.Substring(startIndex2, index2 - startIndex2).Trim();
            }
            return str;
        }
    }
}