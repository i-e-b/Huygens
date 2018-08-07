using System.IO;
using System.Text;

namespace Huygens.Compatibility
{
    /// <summary>
    /// IResponse wrapper for SerialisableResponse
    /// </summary>
    public class SerialisableResponseWrapper : IResponse
    {
        private readonly SerialisableResponse _response;
        private readonly MemoryStream _contentStream;
        private readonly DictionaryArrayHeaderWrapper _headerWrapper;

        /// <summary>
        /// Wrap a response object
        /// </summary>
        public SerialisableResponseWrapper(SerialisableResponse targetResponse)
        {
            _response = targetResponse;
            _contentStream = new MemoryStream();
            _headerWrapper = new DictionaryArrayHeaderWrapper(targetResponse.Headers);
        }

        /// <inheritdoc />
        public int StatusCode { get => _response.StatusCode; set => _response.StatusCode = value; }

        /// <inheritdoc />
        public string ContentType { get => Headers.Get("Content-Type"); set => Headers.Set("Content-Type", value); }

        /// <inheritdoc />
        public Encoding ContentEncoding { get; set; }

        /// <inheritdoc />
        public long ContentLength64 { get => _response.Content.LongLength; set => throw new System.NotImplementedException(); }

        /// <inheritdoc />
        public Stream OutputStream => _contentStream;

        /// <inheritdoc />
        public IHeaderCollection Headers { get => _headerWrapper; set => throw new System.NotImplementedException(); }

        /// <inheritdoc />
        public bool SendChunked { get; set; }

        /// <inheritdoc />
        public string StatusDescription { get => _response.StatusMessage; set => _response.StatusMessage = value; }

        /// <summary>
        /// End the response. Ignored in this implementation
        /// </summary>
        public void Close() { }

        /// <summary>
        /// Build and return the final response object
        /// </summary>
        public SerialisableResponse GetResponse()
        {
            if (_contentStream.Length > 0) {
                _contentStream.Seek(0, SeekOrigin.Begin);
                _response.Content = _contentStream.ToArray();
            }
            return _response;
        }

        /// <inheritdoc />
        public void Redirect(string newTarget)
        {
            _response.StatusCode = 302;
            _response.StatusMessage = "Found";
            Headers.Set("Location", newTarget);
        }

        /// <inheritdoc />
        public void Write(string data, string contentType = "text/plain", int statusCode = 200)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            ContentEncoding = Encoding.UTF8;
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            OutputStream.Write(byteData, 0, byteData.Length);
            Close();
        }

        /// <inheritdoc />
        public void Write(byte[] data, string contentType = "text/plain", int statusCode = 200)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            ContentEncoding = Encoding.UTF8;
            OutputStream.Write(data, 0, data.Length);
            Close();
        }

        /// <inheritdoc />
        public void WriteCompressed(string data, string contentType = "text/plain", int statusCode = 200)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            Headers.Add("Content-Encoding", "gzip");
            byte[] byteData = Encoding.UTF8.GetBytes(data); //TODO: data.GZip();
            ContentLength64 = byteData.Length;
            _contentStream.Write(byteData, 0, byteData.Length);
        }
    }
}