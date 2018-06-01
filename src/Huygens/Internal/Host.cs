//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Huygens.Internal
{
    /// <summary>
    /// Host for IIS site
    /// </summary>  
    public class Host : MarshalByRefObject, IRegisteredObject
    {
        private string _lowerCasedVirtualPath;
        private volatile int _pendingCallsCount;
        private GenericServer _server;

        /// <summary>
        /// Application domain where host is loaded
        /// </summary>
        public AppDomain AppDomain
        {
            get { return AppDomain.CurrentDomain; }
        }

        /// <summary>
        /// Create a host
        /// </summary>
        public Host()
        {
            HostingEnvironment.RegisterObject(this);
        }

        /// <summary>
        /// Turn off directory listing pages
        /// </summary>
        public bool DisableDirectoryListing { get; private set; }

        /// <summary>
        /// Standard ASP.Net path
        /// </summary>
        public string NormalizedClientScriptPath { get; private set; }

        /// <summary>
        /// URL path to hosted site
        /// </summary>
        public string NormalizedVirtualPath { get; private set; }
        
        /// <summary>
        /// Standard ASP.Net path
        /// </summary>
        public string PhysicalClientScriptPath { get; private set; }

        /// <summary>
        /// Path to website files and binaries
        /// </summary>
        public string PhysicalPath { get; private set; }

        /// <summary>
        /// IP port for host, if applicable
        /// </summary>
        public int Port { get; private set; }
        
        /// <summary>
        /// URL path to hosted site
        /// </summary>
        public string VirtualPath { get; private set; }

        /// <summary>
        /// Http host function
        /// </summary>
        void IRegisteredObject.Stop(bool immediate)
        {
            // Unhook the Host so Server will process the requests in the new appdomain.

            if (_server != null)
            {
                _server.HostStopped();
            }

            // Make sure all the pending calls complete before this Object is unregistered.
            WaitForPendingCallsToFinish();

            HostingEnvironment.UnregisterObject(this);

            Thread.Sleep(100);
            HttpRuntime.Close();
            Thread.Sleep(100);
        }


        /// <summary>
        /// Configure host for a given server and paths
        /// </summary>
        public void Configure(GenericServer server, int port, string virtualPath, string physicalPath, bool disableDirectoryListing)
        {
            _server = server;

            Port = port;
            VirtualPath = virtualPath;
            DisableDirectoryListing = disableDirectoryListing;
            _lowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(VirtualPath);
            NormalizedVirtualPath = virtualPath.EndsWith("/", StringComparison.Ordinal)
                                                          ? virtualPath
                                                          : virtualPath + "/";
            NormalizedVirtualPath =
                CultureInfo.InvariantCulture.TextInfo.ToLower(NormalizedVirtualPath);
            PhysicalPath = physicalPath;
            PhysicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + "\\";
            NormalizedClientScriptPath =
                CultureInfo.InvariantCulture.TextInfo.ToLower(HttpRuntime.AspClientScriptVirtualPath + "/");
        }

        /// <summary>Obtains a lifetime service object to control the lifetime policy for this instance.</summary>
        public override object InitializeLifetimeService()
        {
            // never expire the license
            return null;
        }

        /// <summary>
        /// Compare path to the virtual URL path
        /// </summary>
        public bool IsVirtualPathAppPath(string path)
        {
            if (path == null)
            {
                return false;
            }
            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
            return (path == _lowerCasedVirtualPath || path == NormalizedVirtualPath);
        }

        /// <summary>
        /// Test to see if a path is a subpath of the virtual diectory
        /// </summary>
        public bool IsVirtualPathInApp(string path, out bool isClientScriptPath)
        {
            isClientScriptPath = false;

            if (path == null)
            {
                return false;
            }

            if (VirtualPath == "/" && path.StartsWith("/", StringComparison.Ordinal))
            {
                if (path.StartsWith(NormalizedClientScriptPath, StringComparison.Ordinal))
                {
                    isClientScriptPath = true;
                }
                return true;
            }

            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);

            if (path.StartsWith(NormalizedVirtualPath, StringComparison.Ordinal))
            {
                return true;
            }

            if (path == _lowerCasedVirtualPath)
            {
                return true;
            }

            if (path.StartsWith(NormalizedClientScriptPath, StringComparison.Ordinal))
            {
                isClientScriptPath = true;
                return true;
            }

            return false;
        }

        
        /// <summary>
        /// Test to see if a path is a subpath of the virtual diectory
        /// </summary>
        public bool IsVirtualPathInApp(string path)
        {
            return IsVirtualPathInApp(path, out _);
        }

        /// <summary>
        /// Handle a request with the hosted site. Both request and response are pumped through the connection provided
        /// </summary>
        public void ProcessRequest(IConnection conn)
        {
            // Add a pending call to make sure our thread doesn't get killed
            AddPendingCall();

            try
            {
                new Request(_server, this, conn).Process();
            }
            finally
            {
                RemovePendingCall();
            }
        }

        /// <summary>
        /// Shut down the host
        /// </summary>
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public void Shutdown()
        {
            HostingEnvironment.InitiateShutdown();
        }

#pragma warning disable 0420
        private void AddPendingCall()
        {
            Interlocked.Increment(ref _pendingCallsCount);
        }

        private void RemovePendingCall()
        {
            Interlocked.Decrement(ref _pendingCallsCount);
        }
#pragma warning restore 0420

        private void WaitForPendingCallsToFinish()
        {
            for (; ; )
            {
                if (_pendingCallsCount <= 0) { break; }

                Thread.Sleep(250);
            }
        }
    }
}