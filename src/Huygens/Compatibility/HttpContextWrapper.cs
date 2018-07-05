using System.Net;
using System.Web;
using System.Web.SessionState;

namespace Huygens.Compatibility
{
    /// <summary>
    /// IContext wrapper for ASP.Net context
    /// </summary>
    public class HttpContextWrapper : IContext
    {
        /// <inheritdoc />
        public IRequest Request { get { return request; } }

        /// <inheritdoc />
        public IResponse Response { get { return response; } }

        /// <inheritdoc />
        public HttpSessionState Session { get { return context.Session; } }

        /// <inheritdoc />
        public bool IsLocal { get { return context.Request.IsLocal; } }

        /// <inheritdoc />
        public bool IsSecureConnection { get { return context.Request.IsSecureConnection; } }

        /// <summary>
        /// Wrapped request
        /// </summary>
        protected HttpRequestWrapper request;
        /// <summary>
        /// Wrapped context
        /// </summary>
        protected HttpContext context;
        /// <summary>
        /// Wrapped response
        /// </summary>
        protected IResponse response;

        /// <inheritdoc />
        public HttpContextWrapper(HttpContext context)
        {
            this.context = context;
            response = new HttpResponseWrapper(context.Response);
            request = new HttpRequestWrapper(context.Request);
        }

        /// <inheritdoc />
        public IPAddress EndpointAddress()
        {
            return context.EndpointAddress();
        }

        /// <inheritdoc />
        public string Verb()
        {
            return context.Verb();
        }

        /// <inheritdoc />
        public string Path()
        {
            return context.Path();
        }

        /// <inheritdoc />
        public string Extension()
        {
            return context.Extension();
        }

        /// <inheritdoc />
        public string MapPath(string relativePath)
        {
            // Special mapping to make it like the Azure & Commandline versions
            return context.Server.MapPath("~/bin/" + relativePath.TrimStart('/'));
        }

        /// <inheritdoc />
        public void Redirect(string url)
        {
            context.Redirect(url);
        }
    }
}