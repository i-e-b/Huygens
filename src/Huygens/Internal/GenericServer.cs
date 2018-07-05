using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Hosting;

namespace Huygens.Internal
{
    /// <summary>
    /// Basis of server hosts. Pick either DirectServer or SocketServer
    /// </summary>
    public abstract class GenericServer: MarshalByRefObject, IDisposable
    {
        internal ApplicationManager _applicationManager;
        internal AssemblyReflectionManager _reflectionManager;
        internal string AppId { get; set; }
        internal AppDomain HostAppDomain
        {
            get
            {
                if (_host == null) { GetHost(); }
                if (_host != null) { return _host.AppDomain; }
                return null;
            }
        }

        internal object _lockObject;
        internal bool _disposed;

        /// <summary>
        /// Connected host (if any)
        /// </summary>
        protected Host _host;

        /// <summary>
        /// Shutdown lock
        /// </summary>
        protected bool _shutdownInProgress;

        /// <summary>
        /// URL path where hosted application starts. Usually should be "/"
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// Physical path where hosted application is stored
        /// </summary>
        public string PhysicalPath { get; set; }

        ///<summary>
        /// Port if connected to a socket. Dummy value otherwise
        ///</summary>
        public int Port { get; set; }

        ///<summary>
        /// If true, directory browsing will not be available
        ///</summary>
        public bool DisableDirectoryListing { get; set; }

        /// <summary>
        /// Start listening for connections
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stop listening for connections
        /// </summary>
        public abstract void ShutDown();

        /// <summary>
        /// Setup
        /// </summary>
        protected GenericServer()
        {
            _applicationManager = ApplicationManager.GetApplicationManager();
            _reflectionManager = new AssemblyReflectionManager();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                ShutDown();
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Triggered when the host terminated
        /// </summary>
        public virtual void HostStopped()
        {
            _host = null;
        }

        /// <summary>
        /// called at the end of request processing
        /// to disconnect the remoting proxy for Connection object
        /// and allow GC to pick it up
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="userName"></param>
        public virtual void OnRequestEnd(IConnection conn, string userName)
        {
            try
            {
                LogInfo connRequestLogClone = conn.RequestLog.Clone();
                connRequestLogClone.Identity = userName;
                LogInfo connResponseLogClone = conn.ResponseLog.Clone();
                connResponseLogClone.Identity = userName;
                OnRequestComplete(conn.Id, connRequestLogClone, connResponseLogClone);
            }
            catch
            {
                // swallow - we don't want consumer killing the server
            }
            if (conn is MarshalByRefObject mbr) RemotingServices.Disconnect(mbr);
            //DecrementRequestCount();
        }
        
        /// <summary>
        /// Load and configure host if required
        /// </summary>
        public virtual Host GetHost()
        {
            if (_shutdownInProgress) return null;

            var host = _host;
            if (host != null) return host;

            lock (_lockObject)
            {
                host = _host;
                if (host != null) return host;

                host = (Host)CreateWorkerAppDomainWithHost(VirtualPath, PhysicalPath, typeof(Host));
                host.Configure(this, Port, VirtualPath, PhysicalPath, DisableDirectoryListing);
                _host = host;
            }

            return host;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="physicalPath"></param>
        /// <param name="hostType"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is Dmitry's hack to enable running outside of GAC.
        /// There are some errors being thrown when running in proc
        /// </remarks>
        private object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType)
        {
            // create BuildManagerHost in the worker app domain
            //_applicationManager appManager = _applicationManager.GetApplicationManager();
            Type buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
            IRegisteredObject buildManagerHost = _applicationManager.CreateObject(AppId, buildManagerHostType, virtualPath,
                                                                          physicalPath, false);

            // call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
            buildManagerHostType.InvokeMember("RegisterAssembly",
                                              BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                                              null,
                                              buildManagerHost,
                                              new object[] { hostType.Assembly.FullName, hostType.Assembly.Location });

            // create Host in the worker app domain
            // FIXME: getting FileLoadException Could not load file or assembly 'WebDev.WebServer20, Version=4.0.1.6, Culture=neutral, PublicKeyToken=f7f6e0b4240c7c27' or one of its dependencies. Failed to grant permission to execute. (Exception from HRESULT: 0x80131418)
            // when running dnoa 3.4 samples - webdev is registering trust somewhere that we are not
            return _applicationManager.CreateObject(AppId, hostType, virtualPath, physicalPath, false);
        }

        private void OnRequestComplete(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            RequestComplete?.Invoke(this, new RequestEventArgs(id, requestLog, responseLog));
        }

        /// <summary>
        /// Event on request completed
        /// </summary>
        public event EventHandler<RequestEventArgs> RequestComplete;
    }
}