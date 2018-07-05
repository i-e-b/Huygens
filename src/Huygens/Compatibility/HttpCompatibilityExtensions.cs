using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;

namespace Huygens.Compatibility
{
    /// <summary>
    /// Extensions to wrap and unwrap Http context versions
    /// </summary>
    public static class HttpCompatibilityExtensions
    {
        /// <summary>
        /// Wrap an ASP.Net web context
        /// </summary>
        public static IContext Wrap(this HttpContext ctx) {
            return new HttpContextWrapper(ctx);
        }

        /// <summary>
        /// Wrap a .Net Http Listener context
        /// </summary>
        public static IContext Wrap(this HttpListenerContext ctx) {
            return new HttpListenerContextWrapper(ctx);
        }

        /// <summary>
        /// Wrap NameValueCollection headers
        /// </summary>
        public static IHeaderCollection Wrap(this NameValueCollection nvc) {
            return new NameValueCollectionHeaderWrapper(nvc);
        }

        /// <summary>
        /// Unwrap headers to a WebHeaderCollection
        /// </summary>
        public static WebHeaderCollection Unwrap(this IHeaderCollection input) {
            var output = new WebHeaderCollection();
            foreach (var key in input.AllKeys)
            {
                output.Add(key, input[key]);
            }
            return output;
        }

        /// <summary>
        /// Unwrap into an existing NameValueCollection
        /// </summary>
        public static void Unwrap(this NameValueCollection output, IHeaderCollection input)
        {
            output.Clear(); // TODO: check this makes sense?
            foreach (var key in input.AllKeys)
            {
                output.Add(key, input[key]);
            }
        }

        /// <summary>
        /// Determine path
        /// </summary>
        public static string Path(this HttpListenerContext context)
        {
            return context.Request.RawUrl.SubstringBefore("?").SubstringAfter("/").ToLower();
        }
        
        /// <summary>
        /// Determine path
        /// </summary>
        public static string Path(this HttpContext context)
        {
            return context.Request.RawUrl.SubstringBefore("?").SubstringAfter("/").ToLower();
        }
        
        /// <summary>
        /// Determine HTTP verb (also known as Method)
        /// </summary>
        public static string Verb(this HttpListenerContext context)
        {
            return context.Request.HttpMethod.ToUpper();
        }
        
        /// <summary>
        /// Determine HTTP verb (also known as Method)
        /// </summary>
        public static string Verb(this HttpContext context)
        {
            return context.Request.HttpMethod.ToUpper();
        }

        /// <summary>
        /// Guess file extension
        /// </summary>
        public static string Extension(this HttpListenerContext context)
        {
            return context.Path().SubstringAfterLast('.').ToLower();
        }
        
        /// <summary>
        /// Guess file extension
        /// </summary>
        public static string Extension(this HttpContext context)
        {
            return context.Path().SubstringAfterLast('.').ToLower();
        }

        /// <summary>
        /// Guess IP address
        /// </summary>
        public static IPAddress EndpointAddress(this HttpListenerContext context)
        {
            var addr = context.Request.RemoteEndPoint?.Address;
            var ip = addr?.ToString();

            // Handle localhost format.
            if (ip == "::1")
            {
                addr = new IPAddress(new byte[] { 127, 0, 0, 1 });
            }

            return addr;
        }
        
        /// <summary>
        /// Guess IP address
        /// </summary>
        public static IPAddress EndpointAddress(this HttpContext context)
        {
            var addr2 = context.Request.UserHostAddress ?? "0.0.0.0";

            if (context.Request.UserHostAddress == "::1")
            {
                addr2 = "127.0.0.1";
            }

            return new IPAddress(addr2.Split('.').Select(addr => Convert.ToByte(addr)).ToArray());
        }

        /// <summary>
        /// Construct a redirect response
        /// </summary>
        public static void Redirect(this HttpListenerContext context, string url)
        {
            if (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            url = url.Replace('\\', '/');
            var request = context.Request;
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.Redirect;
            var redirectUrl = request.Url.Scheme + "://" + request.Url.Host + "/" + url;
            response.Redirect(redirectUrl);
            response.Close();
        }
        
        /// <summary>
        /// Construct a redirect response
        /// </summary>
        public static void Redirect(this HttpContext context, string url)
        {
            if (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            url = url.Replace('\\', '/');
            var request = context.Request;
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.Redirect;
            var redirectUrl = request.Url.Scheme + "://" + request.Url.Host + "/" + url;
            response.Redirect(redirectUrl, false);
            response.Close();
        }
    }
}