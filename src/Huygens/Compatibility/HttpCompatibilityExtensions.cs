using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
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

        /// <summary>
        /// Parse a query string into a name-value collection
        /// </summary>
        public static NameValueCollection ParseQueryString(this string query)
        {
            var outp = new NameValueCollection();
            FillFromString(query, true, outp);
            return outp;
        }

        // Derived from System.Web.HttpValueCollection
        internal static void FillFromString(string s, bool urlencoded, NameValueCollection coll)
        {
            if (string.IsNullOrWhiteSpace(s)) return;

            int num1 = s.Length;
            for (int index = 0; index < num1; ++index)
            {
                int startIndex = index;
                int num2 = -1;
                for (; index < num1; ++index)
                {
                    switch (s[index])
                    {
                        case '&':
                            goto label_7;
                        case '=':
                            if (num2 < 0) { num2 = index; }
                            break;
                    }
                }
                label_7:
                string str1 = null;
                string str2;
                if (num2 >= 0)
                {
                    str1 = s.Substring(startIndex, num2 - startIndex);
                    str2 = s.Substring(num2 + 1, index - num2 - 1);
                }
                else
                    str2 = s.Substring(startIndex, index - startIndex);
                if (urlencoded)
                    coll.Add(HttpUtility.UrlDecode(str1), HttpUtility.UrlDecode(str2));
                else
                    coll.Add(str1, str2);
                if (index == num1 - 1 && s[index] == 38)
                    coll.Add(null, string.Empty);
            }
        }
    }
}