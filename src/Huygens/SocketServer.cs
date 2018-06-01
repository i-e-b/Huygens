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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Threading;
using System.Web.Hosting;
using Huygens.Internal;

namespace Huygens
{
    ///<summary>
    /// Server that connects to an IP socket and exposes the server outside of the current process
    ///</summary>
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"),
     PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class SocketServer : GenericServer
    {
        private Socket _socket;

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        public SocketServer(int port, string virtualPath, string physicalPath)
            : this(port, virtualPath, physicalPath, false)
        {
        }


        /// <summary>
        /// </summary>
        /// <param name="port"></param>
        /// <param name="virtualPath"></param>
        /// <param name="physicalPath"></param>
        /// <param name="ipAddress"></param>
        /// <param name="hostName"></param>
        /// <param name="disableDirectoryListing"></param>
        public SocketServer(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName, bool disableDirectoryListing)
            : this(port, virtualPath, physicalPath, disableDirectoryListing)
        {
            IPAddress = ipAddress;
            HostName = hostName;
        }

        /// <summary>
        /// </summary>
        /// <param name="port"></param>
        /// <param name="virtualPath"></param>
        /// <param name="physicalPath"></param>
        /// <param name="disableDirectoryListing"></param>
        public SocketServer(int port, string virtualPath, string physicalPath, bool disableDirectoryListing)
        {
            IPAddress = IPAddress.Loopback;
            DisableDirectoryListing = disableDirectoryListing;
            _lockObject = new object();
            Port = port;
            VirtualPath = virtualPath;
            PhysicalPath = Path.GetFullPath(physicalPath);
            PhysicalPath = PhysicalPath.EndsWith("\\", StringComparison.Ordinal)
                                ? PhysicalPath
                                : PhysicalPath + "\\";

            _applicationManager = ApplicationManager.GetApplicationManager();
            string uniqueAppString = string.Concat(virtualPath, physicalPath,":",Port.ToString()).ToLowerInvariant();
            AppId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);
        }



        ///<summary>
        ///</summary>
        public string HostName { get; }

        ///<summary>
        ///</summary>
        public IPAddress IPAddress { get; }

        ///<summary>
        ///</summary>
        public string RootUrl
        {
            get
            {
                string hostname = HostName;
                if (string.IsNullOrEmpty(HostName))
                {
                    if (IPAddress.Equals(IPAddress.Loopback) || IPAddress.Equals(IPAddress.IPv6Loopback) ||
                        IPAddress.Equals(IPAddress.Any) || IPAddress.Equals(IPAddress.IPv6Any))
                    {
                        hostname = "localhost";
                    }
                    else
                    {
                        hostname = IPAddress.ToString();
                    }
                }

                return Port != 80
                           ?  String.Format("http://{0}:{1}{2}", hostname, Port, VirtualPath)
                           : string.Format("http://{0}{1}", hostname, VirtualPath);
            }
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"/> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> property.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. 
        ///                 </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/></PermissionSet>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            // never expire the license
            return null;
        }


        /// <inheritdoc />
        public override void Start()
        {
            _socket = CreateSocketBindAndListen(AddressFamily.InterNetwork, IPAddress, Port);

            ThreadPool.QueueUserWorkItem(delegate
                {
                    while (!_shutdownInProgress)
                    {
                        try
                        {
                            Socket acceptedSocket = _socket.Accept();

                            ThreadPool.QueueUserWorkItem(delegate
                                {
                                    if (!_shutdownInProgress)
                                    {
                                        var conn = new SocketConnection(this, acceptedSocket);

                                        if (conn.WaitForRequestBytes() == 0)
                                        {
                                            conn.WriteErrorAndClose(400);
                                            return;
                                        }

                                        Host host = GetHost();

                                        if (host == null)
                                        {
                                            conn.WriteErrorAndClose(500);
                                            return;
                                        }

                                        host.ProcessRequest(conn);
                                    }
                                });
                        }
                        catch
                        {
                            Thread.Sleep(100);
                        }
                    }
                });
        }


        /// <summary>
        /// Allows an <see cref="T:System.Object"/> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Object"/> is reclaimed by garbage collection.
        /// </summary>
        ~SocketServer()
        {
            Dispose();
        }


        private static Socket CreateSocketBindAndListen(AddressFamily family, IPAddress address, int port)
        {
            Socket socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(address, port));
            socket.Listen((int)SocketOptionName.MaxConnections);
            return socket;
        }

        /// <inheritdoc />
        public override void ShutDown()
        {
            if (_shutdownInProgress)
            {
                return;
            }

            _shutdownInProgress = true;

            try
            {
                _socket?.Close();
            }
            catch { Ignore(); }
            finally
            {
                _socket = null;
            }

            try
            {
                _host?.Shutdown();

                // the host is going to raise an event that this class uses to null the field.
                // just wait until the field is nulled and continue.

                while (_host != null)
                {
                    new AutoResetEvent(false).WaitOne(100);
                }
            }
            catch { Ignore(); }
  
        }

        private void Ignore() { }
    }
}