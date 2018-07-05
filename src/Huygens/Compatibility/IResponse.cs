using System.IO;
using System.Text;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Interface for server responses
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Status of response
        /// </summary>
        int StatusCode { get; set; }

        /// <summary>
        /// MIME type of content being returned
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Data encoding of content being returned
        /// </summary>
        Encoding ContentEncoding { get; set; }

        /// <summary>
        /// Byte count of content returned, excluding headers
        /// </summary>
        long ContentLength64 { get; set; }

        /// <summary>
        /// Output data stream
        /// </summary>
        Stream OutputStream { get; }

        /// <summary>
        /// Response headers
        /// </summary>
        IHeaderCollection Headers { get; set; }

        /// <summary>
        /// Send as chunks (otherwise as a predefined length)
        /// </summary>
        bool SendChunked { get; set; }

        /// <summary>
        /// Description of status code
        /// </summary>
        string StatusDescription { get; set; }

        /// <summary>
        /// End the response
        /// </summary>
        void Close();

        /// <summary>
        /// Write data to the client
        /// </summary>
        void Write(string data, string contentType = "text/plain", int statusCode = 200);
        
        /// <summary>
        /// Write data to the client
        /// </summary>
        void WriteCompressed(string data, string contentType = "text/plain", int statusCode = 200);
        
        /// <summary>
        /// Write data to the client
        /// </summary>
        void Write(byte[] data, string contentType = "text/plain", int statusCode = 200);

        /// <summary>
        /// Build a redirect response
        /// </summary>
        /// <param name="newTarget"></param>
        void Redirect(string newTarget);
    }
}