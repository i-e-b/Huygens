using System.IO;
using System.Net;
using System.Text;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Wraps a .Net listener response
    /// </summary>
    public class HttpListenerResponseWrapper : IResponse
    {
        /// <inheritdoc />
        public int StatusCode { get { return response.StatusCode; } set { response.StatusCode = value; } }

        /// <inheritdoc />
        public string ContentType { get { return response.ContentType; } set { response.ContentType = value; } }

        /// <inheritdoc />
        public Encoding ContentEncoding { get { return response.ContentEncoding; } set { response.ContentEncoding = value; } }

        /// <inheritdoc />
        public long ContentLength64 { get { return response.ContentLength64; } set { response.ContentLength64 = value; } }

        /// <inheritdoc />
        public Stream OutputStream { get { return response.OutputStream; } }

        /// <inheritdoc />
        public IHeaderCollection Headers { get{ return response.Headers.Wrap(); } set { response.Headers = value.Unwrap(); } }

        /// <inheritdoc />
        public bool SendChunked { get { return response.SendChunked; } set { response.SendChunked = value; } }

        /// <inheritdoc />
        public string StatusDescription { get{return response.StatusDescription; } set{ response.StatusDescription = value; } }

        /// <summary>
        /// Wrapped response
        /// </summary>
        protected HttpListenerResponse response;

        /// <summary>
        /// Wrap a .Net listener response
        /// </summary>
        public HttpListenerResponseWrapper(HttpListenerResponse response)
        {
            this.response = response;
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
        public void WriteCompressed(string data, string contentType = "text/plain", int statusCode = 200)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            response.AddHeader("Content-Encoding", "gzip");
            byte[] byteData = Encoding.UTF8.GetBytes(data); //TODO: data.GZip();
            ContentLength64 = byteData.Length;
            response.OutputStream.Write(byteData, 0, byteData.Length);
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
        public void Redirect(string newTarget)
        {
            response.Redirect(newTarget);
        }

        /// <inheritdoc />
        public void Close()
        {
            response.Close();
        }
    }
}