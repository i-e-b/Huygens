using System;
using System.Globalization;
using System.IO;
using Huygens.Internal;

namespace Huygens
{
    /// <summary>
    /// Direct server hosts a site, allowing direct calling of endpoints.
    /// This does NOT expose any IP sockets, and does not require IP port permissions.
    /// <para/>
    /// This can be used for internal hosting, proxing and integration testing
    /// </summary>
    public class DirectServer : GenericServer
    {
        /// <summary>
        /// Host an IIS web site in memory
        /// </summary>
        /// <param name="physicalPath">Path to the site (including any Global.asax & web.config files)</param>
        /// <param name="enableDirectoryListing">If true, expose directory browsing pages. Defaults to false.</param>
        public DirectServer(string physicalPath, bool enableDirectoryListing = false)
        {
            DisableDirectoryListing = !enableDirectoryListing;
            _lockObject = new object();
            Port = 0;
            VirtualPath = "/";
            PhysicalPath = Path.GetFullPath(physicalPath);
            PhysicalPath = PhysicalPath.EndsWith("\\", StringComparison.Ordinal)
                ? PhysicalPath
                : PhysicalPath + "\\";

            string uniqueAppString = string.Concat("/", physicalPath, ":", Port.ToString()).ToLowerInvariant();
            AppId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Supply a request call to the hosted site, and get back a response.
        /// This is done synchronously.
        /// </summary>
        /// <param name="request">Request from client</param>
        /// <returns>Response from server</returns>
        public SerialisableResponse DirectCall(SerialisableRequest request)
        {
            var conn = new MemoryConnection(request);
            GetHost()?.ProcessRequest(conn);
            return conn.GenerateResponse();
        }

        /// <summary>
        /// Start listening for messages. Does nothing in the Direct Server
        /// </summary>
        public override void Start() { }
        
        /// <summary>
        /// Stop listening for messages. Does nothing in the Direct Server
        /// </summary>
        public override void ShutDown() { }
    }
}