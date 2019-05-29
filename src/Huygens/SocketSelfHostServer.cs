using Huygens.Internal;

namespace Huygens
{
    /// <summary>
    /// Server that connects to an IP socket and exposes the server outside of the current process.
    /// <para></para>
    /// This host does not use an existing published site, but expects you to provide an IHost implementation
    /// </summary>
    public class SocketSelfHostServer: SocketServer {
        private readonly IHost _originalHost;

        /// <summary>
        /// Connect a host implementation to an external port
        /// </summary>
        public SocketSelfHostServer(int port, IHost host)
            : base(port, "", null, true)
        {
            _originalHost = _host = host;
        }

        /// <inheritdoc />
        public override IHost GetHost()
        {
            return _originalHost;
        }

        /// <inheritdoc />
        protected override void PostShutdown()
        {
            _host = null; // required for the Generic server's shut-down pause.
        }
    }
}