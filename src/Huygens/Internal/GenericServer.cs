using System;
using System.IO;
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
        /// <summary>
        /// Application manager instance
        /// </summary>
        protected ApplicationManager _applicationManager;

        /// <summary>
        /// Reflection manager instance
        /// </summary>
        protected AssemblyReflectionManager _reflectionManager;

        /// <summary>
        /// Application ID
        /// </summary>
        public string AppId { get; internal set; }

        /// <summary>
        /// App domain hosting the site
        /// </summary>
        public AppDomain HostAppDomain
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
        protected IHost _host;

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
        public void OnRequestEnd(IConnection conn, string userName)
        {
            try
            {
                var connRequestLogClone = conn.RequestLog?.Clone();
                if (connRequestLogClone != null) connRequestLogClone.Identity = userName;
                var connResponseLogClone = conn.ResponseLog?.Clone();
                if (connResponseLogClone != null) connResponseLogClone.Identity = userName;
                if (RequestComplete != null) OnRequestComplete(conn.Id, connRequestLogClone, connResponseLogClone);
            }
            catch
            {
                // swallow - we don't want consumer killing the server
            }
            if (conn is MarshalByRefObject mbr) RemotingServices.Disconnect(mbr);
        }
        
        /// <summary>
        /// Load and configure host if required
        /// </summary>
        public virtual IHost GetHost()
        {
            if (_shutdownInProgress) return null;

            var host = _host;
            if (host != null)
            {
                try
                {
                    if (host.DomainLoaded) return host;
                    _host = null;
                }
                catch (AppDomainUnloadedException)
                {
                    _host = null;
                }
            }

            lock (_lockObject)
            {
                host = _host;
                if (host != null) return host;

                object proxy = null;
                try
                {
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    proxy = CreateWorkerAppDomainWithHost(VirtualPath, PhysicalPath, typeof(Host));
                    host = (Host)proxy;
                }
                catch (InvalidCastException ex)
                {
                    throw new ApplicationException("Reflection casting exception in CreateWorkerAppDomainWithHost. Possibly bad AppDomain paths.\r\n"
                        + GetVersionInfo(typeof(Host))
                        + GetVersionInfo(proxy?.GetType()), ex);
                }
                finally {
                    AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                }
                host.Configure(this, Port, VirtualPath, PhysicalPath, DisableDirectoryListing);
                _host = host;
            }

            return host;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assembly = Assembly.Load(args.Name);
                if (assembly != null) return assembly;
            }
            catch
            { 
                // Ignore
            }

            var parts = args.Name.Split(',');
            var file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + parts[0].Trim() + ".dll";

            return Assembly.LoadFrom(file);
        }

        private string GetVersionInfo(Type target)
        {
            return "Type name: " + target.FullName + "\r\n"+
                   ".NET Version: " + Environment.Version + "\r\n" +
                   "Reflection Assembly: " + target.Assembly.CodeBase.Replace("file:///", "").Replace("/", "\\") + "\r\n" +
                   "Assembly Cur Dir: " + Directory.GetCurrentDirectory() + "\r\n" +
                   "ApplicationBase: " + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\r\n" +
                   "GetExecutingAssembly: " + Assembly.GetExecutingAssembly().Location + "\r\n" +
                   "App Domain: " + AppDomain.CurrentDomain.FriendlyName + "\r\n";
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
        protected object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType)
        {
            // If the executing process is named "w3wp.exe", ASP.Net will fail to run with an access violation error.
            // We 'break' the cached name of the exe here to work around that issue.
            // If we fail to break, we will continue, but you may get the issue. If you have an exception at System.Web.Hosting.UnsafeIISMethods...
            // then you will need to fix this again.
            var versionInfoType = typeof(HttpApplication).Assembly.GetType("System.Web.Util.VersionInfo"); // find the hidden type
            var exeNameField = versionInfoType?.GetField("_exeName", BindingFlags.Static | BindingFlags.NonPublic); // grab the static cache
            exeNameField?.SetValue(null, "AnythingElse.exe"); // rename it before we trigger the initial setup.


            // create BuildManagerHost in the worker app domain
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

        /// <summary>
        /// Request complete invoker
        /// </summary>
        protected void OnRequestComplete(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            RequestComplete?.Invoke(this, new RequestEventArgs(id, requestLog, responseLog));
        }

        /// <summary>
        /// Event on request completed
        /// </summary>
        public event EventHandler<RequestEventArgs> RequestComplete;
    }
}