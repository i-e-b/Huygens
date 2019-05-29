using System;
using System.Net;
using Huygens;
using Huygens.Internal;
using NUnit.Framework;

namespace HuygensTests
{
    [TestFixture]
    public class SocketSelfHostTests{
        [Test]
        public void hosting_in_function() {
            var host = new TestHost();
            using (var subject = new SocketSelfHostServer(8082, host)) {
                subject.Start();

                // Make a call out to the server:
                var c = new WebClient();
                var result = c.DownloadString("http://localhost:8082");

                subject.ShutDown();

                Assert.That(result, Is.EqualTo("Hello, world"));
            }
        }

public class TestHost : IHost
{
    /// <inheritdoc />
    public void ProcessRequest(IConnection conn)
    {
        conn.WriteEntireResponseFromString(200, null, "Hello, world", false);
    }

            /// <inheritdoc />
            public void Stop(bool immediate) { }
            /// <inheritdoc />
            public AppDomain AppDomain { get; }
            /// <inheritdoc />
            public bool DomainLoaded { get; set; }
            /// <inheritdoc />
            public bool DisableDirectoryListing { get; }
            /// <inheritdoc />
            public string NormalizedClientScriptPath { get; }
            /// <inheritdoc />
            public string NormalizedVirtualPath { get; }
            /// <inheritdoc />
            public string PhysicalClientScriptPath { get; }
            /// <inheritdoc />
            public string PhysicalPath { get; }
            /// <inheritdoc />
            public int Port { get; }
            /// <inheritdoc />
            public string VirtualPath { get; }

            /// <inheritdoc />
            public void Configure(GenericServer server, int port, string virtualPath, string physicalPath, bool disableDirectoryListing) { }

            /// <inheritdoc />
            public bool IsVirtualPathAppPath(string path)=>true;

            /// <inheritdoc />
            public bool IsVirtualPathInApp(string path, out bool isClientScriptPath) {
                isClientScriptPath = true;
                return true;
            }

            /// <inheritdoc />
            public bool IsVirtualPathInApp(string path)=>true;

            /// <inheritdoc />
            public void Shutdown() { }
        }
    }
}