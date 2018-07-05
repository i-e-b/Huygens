using System;
using System.Net;
using System.Web.SessionState;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Wrapper for .Net http listener
    /// </summary>
    public class HttpListenerContextWrapper : IContext
    {
        /// <inheritdoc />
        public HttpSessionState Session { get { throw new ApplicationException("Please use IWebSessionService."); } }

        /// <inheritdoc />
        public IRequest Request { get { return request; } }

        /// <inheritdoc />
        public IResponse Response { get { return response; } }

        /// <inheritdoc />
        public bool IsLocal { get { return context.Request.IsLocal; } }

        /// <inheritdoc />
        public bool IsSecureConnection { get { return context.Request.IsSecureConnection; } }

        /// <summary>
        /// Wrapped request
        /// </summary>
        protected HttpListenerRequestWrapper request;
        /// <summary>
        /// Wrapped response
        /// </summary>
        protected HttpListenerResponseWrapper response;
        /// <summary>
        /// wrapped context
        /// </summary>
        protected HttpListenerContext context;

        /// <summary>
        /// wrap a listener context
        /// </summary>
        public HttpListenerContextWrapper(HttpListenerContext context)
        {
            this.context = context;
            response = new HttpListenerResponseWrapper(context.Response);
            request = new HttpListenerRequestWrapper(context.Request);
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
            return relativePath;
        }

        /// <inheritdoc />
        public void Redirect(string url)
        {
            context.Redirect(url);
        }
    }
}