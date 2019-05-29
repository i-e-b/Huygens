using System;
using System.Web.Hosting;

namespace Huygens.Internal
{
    /// <summary>
    /// Interface for a host that can respond to requests
    /// </summary>
    public interface IHost : IRegisteredObject
    {
        /// <summary>
        /// Application domain where host is loaded
        /// </summary>
        AppDomain AppDomain { get; }

        /// <summary>
        /// True until an app domain unloaded event is fired
        /// </summary>
        bool DomainLoaded { get; set; }

        /// <summary>
        /// Turn off directory listing pages
        /// </summary>
        bool DisableDirectoryListing { get; }

        /// <summary>
        /// Standard ASP.Net path
        /// </summary>
        string NormalizedClientScriptPath { get; }

        /// <summary>
        /// URL path to hosted site
        /// </summary>
        string NormalizedVirtualPath { get; }

        /// <summary>
        /// Standard ASP.Net path
        /// </summary>
        string PhysicalClientScriptPath { get; }

        /// <summary>
        /// Path to website files and binaries
        /// </summary>
        string PhysicalPath { get; }

        /// <summary>
        /// IP port for host, if applicable
        /// </summary>
        int Port { get; }

        /// <summary>
        /// URL path to hosted site
        /// </summary>
        string VirtualPath { get; }

        /// <summary>
        /// Configure host for a given server and paths
        /// </summary>
        void Configure(GenericServer server, int port, string virtualPath, string physicalPath, bool disableDirectoryListing);

        /// <summary>
        /// Compare path to the virtual URL path
        /// </summary>
        bool IsVirtualPathAppPath(string path);

        /// <summary>
        /// Test to see if a path is a subpath of the virtual diectory
        /// </summary>
        bool IsVirtualPathInApp(string path, out bool isClientScriptPath);

        /// <summary>
        /// Test to see if a path is a subpath of the virtual diectory
        /// </summary>
        bool IsVirtualPathInApp(string path);

        /// <summary>
        /// Handle a request with the hosted site. Both request and response are pumped through the connection provided
        /// </summary>
        void ProcessRequest(IConnection conn);

        /// <summary>
        /// Shut down the host
        /// </summary>
        void Shutdown();
    }
}