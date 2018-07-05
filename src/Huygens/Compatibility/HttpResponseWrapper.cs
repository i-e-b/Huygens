using System.IO;
using System.Text;
using System.Web;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Wrap an ASP.Net response
    /// </summary>
    public class HttpResponseWrapper : IResponse
    {
        /// <inheritdoc />
        public int StatusCode { get { return response.StatusCode; } set { response.StatusCode = value; } }

        /// <inheritdoc />
        public string ContentType { get { return response.ContentType; } set { response.ContentType = value; } }

        /// <inheritdoc />
        public Encoding ContentEncoding { get { return response.ContentEncoding; } set { response.ContentEncoding = value; } }

        /// <inheritdoc />
        public long ContentLength64 { get { return contentLength; } set { contentLength = value; } }

        /// <inheritdoc />
        public Stream OutputStream { get { return response.OutputStream; } }

        /// <inheritdoc />
        public IHeaderCollection Headers { get { return response.Headers.Wrap(); } set{ response.Headers.Unwrap(value); } }

        /// <inheritdoc />
        public bool SendChunked { get{ return false; } set { /* not supported */ } }

        /// <inheritdoc />
        public string StatusDescription { get { return response.StatusDescription; } set { response.StatusDescription = value; } }

        /// <summary>
        /// Wrapped response
        /// </summary>
        protected HttpResponse response;
        /// <summary>
        /// Content length
        /// </summary>
        protected long contentLength;

        /// <summary>
        /// Wrap an ASP.Net response
        /// </summary>
        public HttpResponseWrapper(HttpResponse response)
        {
            this.response = response;
        }

        /// <inheritdoc />
        public void Close()
        {
            // Never close the response from IIS.
            // response.End();
        }

        /// <inheritdoc />
        public void Write(string data, string contentType = "text/plain", int statusCode = 200)
        {
            // IIS will fire the EndRequest event twice, the second time, setting header information will throw an exception.
            // This is handled by the IISService, testing for context.Response.HeadersWritten to prevent processing of the second
            // request.
            StatusCode = statusCode;
            ContentType = contentType;
            ContentEncoding = Encoding.UTF8;
            var byteData = Encoding.UTF8.GetBytes(data);
            response.OutputStream.Write(byteData, 0, byteData.Length);
            Close();
        }

        /// <inheritdoc />
        public void WriteCompressed(string data, string contentType = "text/plain", int statusCode = 200)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            response.AddHeader("Content-Encoding", "gzip");
            var byteData = Encoding.UTF8.GetBytes(data);// TODO: .GZip();
            response.OutputStream.Write(byteData, 0, byteData.Length);
            Close();
        }

        /// <inheritdoc />
        public void Write(byte[] data, string contentType = "text/plain", int statusCode = 200)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            ContentEncoding = Encoding.UTF8;
            response.OutputStream.Write(data, 0, data.Length);
            Close();
        }

        /// <inheritdoc />
        public void Redirect(string newTarget)
        {
            response.Redirect(newTarget);
        }
    }
}