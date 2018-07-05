using System.Net;
using System.Web.SessionState;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Abstraction for a http request/response
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Endpoint
        /// </summary>
        IPAddress EndpointAddress();

        /// <summary>
        /// Http verb (also known as Method)
        /// </summary>
        string Verb();

        /// <summary>
        /// Path segment of URL
        /// </summary>
        /// <returns></returns>
        string Path();

        /// <summary>
        /// Guess of file extension from url
        /// </summary>
        string Extension();

        /// <summary>
        /// Request interface
        /// </summary>
        IRequest Request { get; }

        /// <summary>
        /// Response interface
        /// </summary>
        IResponse Response { get; }

        /// <summary>
        /// Session state if available. You should generally avoid this.
        /// </summary>
        HttpSessionState Session { get; }

        /// <summary>
        /// Is this a localhost
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        /// Is this a https connection
        /// </summary>
        bool IsSecureConnection { get; }

        /// <summary>
        /// Guess the physical path for a url
        /// </summary>
        string MapPath(string relativePath);

        /// <summary>
        /// Build a redirect response
        /// </summary>
        void Redirect(string url);
    }
}