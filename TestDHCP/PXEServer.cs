using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace TestDHCP
{
    public class PXEServer : IDisposable
    {
        public IPAddress BindAddress { get; set; }
        public IPAddress NetMask { get; set; }

        public int DHCPPort { get; set; }

        private DHCPServer dhcp_server;

        private Loader loader;

        public PXEServer(PXEConfig config)
        {
            // = DHCP
            DHCPPort = config.DHCPPort;
            BindAddress = IPAddress.Parse(config.BindAddress);
            NetMask = IPAddress.Parse(config.NetMask);

            // = PXE
            loader = Enum.Parse<Loader>(config.Loader);

            if (config.Verbose)
            {
                Trace.Listeners.Add(new ConsoleTraceListener());
            }

        }
        public void Start()
        {

            Stop();

            // = DHCP
            if (DHCPPort > 0)
            {
                var net = new IPSegment(BindAddress.ToString(), NetMask.ToString());

                dhcp_server = new DHCPServer(BindAddress, DHCPPort);

                dhcp_server.Loader = loader;
                //dhcp_server.HTTPBootFile = HTTPBootFile;
                //dhcp_server.SubnetMask = IPAddress.Parse("255.255.255.0");  // = MISSING CODE: Generic subnet mask from current net
                dhcp_server.PoolStart = net.Hosts().First().ToIpAddress();
                dhcp_server.PoolEnd = net.Hosts().Last().ToIpAddress();

                dhcp_server.Start();

                Console.WriteLine("DHCP server is running. Press any key to exit the server.");
                Console.ReadKey();
            }

        } 

        public void Stop()
        { 
            dhcp_server?.Dispose();
        }

        #region dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
