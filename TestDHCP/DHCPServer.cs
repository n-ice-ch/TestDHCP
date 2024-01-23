using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GitHub.JPMikkers.DHCP;

namespace TestDHCP
{
    public class DHCPServer : GitHub.JPMikkers.DHCP.DHCPServer
    {
        //private static readonly ILog _log = LogProvider.GetCurrentClassLogger();

        public IPAddress BindAddress { get; set; }

        public Loader Loader { get; set; } = Loader.IPXE;
        public string HTTPBootFile { get; set; }

        public DHCPServer(IPAddress address, int port) : base(null)
        {
            InitializeServer(address, port);
        }

        public DHCPServer(int port) : base(null)
        {
            InitializeServer(IPAddress.Any, port);
        }

        private void InitializeServer(IPAddress address, int port)
        {
            //_log.Debug("BLUB");

            this.BindAddress = address;
            this.EndPoint = new IPEndPoint(address, port);
            this.SubnetMask = IPAddress.Parse("255.255.255.0");
            this.PoolStart = IPAddress.Parse("192.168.1.250");
            this.PoolEnd = IPAddress.Parse("192.168.1.253");
            this.LeaseTime = GitHub.JPMikkers.DHCP.Utils.InfiniteTimeSpan;
            this.OfferExpirationTime = TimeSpan.FromSeconds(30);
            this.MinimumPacketSize = 576;

            this.OnStatusChange += Dhcpd_OnStatusChange;
            this.OnTrace += Dhcpd_OnTrace;

            Options.Add(new GitHub.JPMikkers.DHCP.OptionItem(mode: GitHub.JPMikkers.DHCP.OptionMode.Force,
                option: new GitHub.JPMikkers.DHCP.DHCPOptionRouter()
                {
                    IPAddresses = new[] { address }
                }));

            Options.Add(new GitHub.JPMikkers.DHCP.OptionItem(mode: GitHub.JPMikkers.DHCP.OptionMode.Force,
                option: new GitHub.JPMikkers.DHCP.DHCPOptionServerIdentifier(address)));

            Options.Add(new GitHub.JPMikkers.DHCP.OptionItem(mode: GitHub.JPMikkers.DHCP.OptionMode.Force,
                option: new GitHub.JPMikkers.DHCP.DHCPOptionTFTPServerName(Dns.GetHostName())));

            Options.Add(new GitHub.JPMikkers.DHCP.OptionItem(mode: GitHub.JPMikkers.DHCP.OptionMode.Force,
                option: new GitHub.JPMikkers.DHCP.DHCPOptionHostName(Dns.GetHostName())));
        }


        // = https://www.ietf.org/assignments/dhcpv6-parameters/dhcpv6-parameters.xml#processor-architecture
        // = https://www.iana.org/assignments/dhcpv6-parameters/dhcpv6-parameters.xhtml#processor-architecture
        private readonly Dictionary<(Loader, byte), string> avalibleArch = new Dictionary<(Loader, byte), string>()
        {

            // = "x86-x64 BIOS"
            { (Loader.SYSLINUX,00),"syslinux\\pxelinux.0" },

            // = "x86 UEFI"
            { (Loader.SYSLINUX,06),"syslinux\\syslinux32.efi" },

            // = "x64 UEFI"
            { (Loader.SYSLINUX,07),"syslinux\\syslinux64.efi" },


            // https://kenvb.gitbook.io/windows-deployment-services/technisch/boot-files

            // = "x86-x64 BIOS"
            { (Loader.MICROSOFT,00),"microsoft\\boot\\pxeboot.n12" },  // pxeboot.x64 <-- Is booting but does not work because for timeout

            // = "x86 UEFI"
            { (Loader.MICROSOFT,06),"microsoft\\boot\\bootmgfw.efi" },

            // = "x64 UEFI"
            { (Loader.MICROSOFT,07),"microsoft\\boot\\bootmgfw.efi" },  // "syslinux64.efi"  "boot\\bootmgfw.efi"


            // = "x86-x64 BIOS"
            { (Loader.IPXE,00),"ipxe\\undionly.kpxe" },

            // = "x64 UEFI"
            { (Loader.IPXE,07),"ipxe\\snponly.efi" },

            // = "x86-x64 BIOS"
            { (Loader.SHIM_GRUB2,00),"grub2.pxe" },

            // = "x64 UEFI"
            { (Loader.SHIM_GRUB2,07),"shimx64.efi" },

            // = x64 UEFI HTTP
            { (Loader.UEFI_HTTP,07),"shimx64.efi" }

        };

        /*
        protected override void ProcessingReceiveMessage(DHCPMessage sourceMsg, DHCPMessage targetMsg)
        {
            var bootFile = string.Empty;


            if (sourceMsg.isHTTP())
            {
                bootFile = HTTPBootFile;
            }
            else
            if (sourceMsg.isIPXE())
            {
                bootFile = HTTPBootFile;
            }
            else


            if (sourceMsg.isPXE())
            {
                var arch = sourceMsg.GetArch();
                bootFile = avalibleArch[(Loader, arch)];
            }

            targetMsg.BootFileName = bootFile;
            targetMsg.NextServerIPAddress = BindAddress;
        }
        */

        public new void Start()
        {
            base.Start();
        }

        private void Dhcpd_OnTrace(object sender, GitHub.JPMikkers.DHCP.DHCPTraceEventArgs e)
        {
            Trace.WriteLine(e?.Message);
            Trace.Flush();
        }

        private void Dhcpd_OnStatusChange(object sender, GitHub.JPMikkers.DHCP.DHCPStopEventArgs e)
        {
            Trace.WriteLine(e?.Reason);
            Trace.Flush();
        }

    }


    public static class DHCPMessageExtensions
    {
        public static byte GetArch(this DHCPMessage message)
        {
            try
            {
                return message.Options
             .Where(x => x.OptionType == TDHCPOption.ClientSystemArchitectureType)
             .Cast<DHCPOptionGeneric>()
             .Select(x => x.Data[1])
             .First();
            }
            catch { return 0; }
        }

        public static string GetVendorClass(this DHCPMessage message)
        {
            var sb = new StringBuilder();
            var strings = message.Options
             .Where(x => x.OptionType == TDHCPOption.VendorClassIdentifier)
             .Cast<DHCPOptionVendorClassIdentifier>()
             .Select(x => Encoding.ASCII.GetString(x.Data));

            try
            {
                foreach (var s in strings)
                {
                    sb.AppendLine(s);
                }
            }
            catch { }
            return sb.ToString();
        }
        public static bool isHTTP(this DHCPMessage message) => GetVendorClass(message).Contains("HTTPClient");
        public static bool isPXE(this DHCPMessage message) => GetVendorClass(message).Contains("PXEClient");
        public static bool isIPXE(this DHCPMessage message)
        {
            try
            {
                return message.Options
                    .Where(x => x.OptionType == (TDHCPOption)77)
                    .Cast<DHCPOptionGeneric>()
                    .Select(x => Encoding.ASCII.GetString(x.Data))
                    .Any(x => x.Equals("iPXE", StringComparison.InvariantCultureIgnoreCase));
            }
            catch { return false; }
        }

    }
}
