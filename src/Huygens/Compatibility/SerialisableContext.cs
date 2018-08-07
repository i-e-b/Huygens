using System.Net;
using System.Web.SessionState;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Context implementation for Serialisable request and response.
    /// This wrapper is not serialisable in itself.
    /// </summary>
    public class SerialisableContext : IContext
    {
        /// <summary>
        /// Create a new context for an incoming serialisable request
        /// </summary>
        public SerialisableContext(SerialisableRequest request)
        {
            Request = new SerialisableRequestWrapper(request);

            Response = new SerialisableResponseWrapper(SerialisableResponse.CreateEmpty());
        }

        /// <summary>
        /// Access the underlying serialisable response object
        /// </summary>
        public SerialisableResponse GetResponse()
        {
            return ((SerialisableResponseWrapper)Response).GetResponse();
        }

        /// <inheritdoc />
        public string Verb()
        {
            return Request.HttpMethod;
        }

        /// <inheritdoc />
        public string Path()
        {
            return Request.RawUrl.SubstringBefore("?").SubstringAfter("/").ToLower();
        }

        /// <inheritdoc />
        public string Extension()
        {
            return Path().SubstringAfterLast('.').ToLower();
        }

        /// <inheritdoc />
        public IRequest Request { get; }

        /// <inheritdoc />
        public IResponse Response { get; }

        /// <inheritdoc />
        public HttpSessionState Session { get; } = null;

        /// <inheritdoc />
        public bool IsLocal { get; } = true;

        /// <inheritdoc />
        public bool IsSecureConnection => Request.IsSecureConnection;


        /// <summary>
        /// Guess the physical path for a url.
        /// This version does nothing, and returns back the relative path
        /// </summary>
        public string MapPath(string relativePath)
        {
            return relativePath;
        }

        /// <inheritdoc />
        public void Redirect(string url)
        {
            Response.StatusCode = 302;
            Response.StatusDescription = "Found";
            Response.Headers.Set("Location", url);
        }

        /// <summary>
        /// Endpoint address. This implementation always gives the loopback address
        /// </summary>
        public IPAddress EndpointAddress()
        {
            return IPAddress.Loopback;
        }
    }
}