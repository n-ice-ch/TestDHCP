using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestDHCP
{
    public class PXEConfig
    {
        public string BindAddress { get; set; } = "192.168.2.155";
        public string NetMask { get; set; } = "255.255.255.0";

        public bool Verbose { get; set; } = true;

        public int DHCPPort { get; set; } = 67;
        public int HTTPPort { get; set; } = 80;


        public string ServerDirectory { get; set; } = "";
        public string Loader { get; set; } = "IPXE";

        public string HTTPBootFile { get; set; }

        // = TFTP
        public int TFTPPort { get; set; } = 69;
        public int TFTPWindowSize { get; set; } = 8;

        #region save/load
        const string cfg_file_name = "TestDHCP.conf";
        static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            WriteIndented = true
        };

        public static PXEConfig Load()
        {
            try
            {
                var bytes = File.ReadAllBytes(cfg_file_name);
                return JsonSerializer.Deserialize<PXEConfig>(bytes, jsonSerializerOptions);

            }
            catch { return new PXEConfig(); }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize<PXEConfig>(this, jsonSerializerOptions);
                File.WriteAllText(cfg_file_name, json);
            }
            catch { }
        }

        public class PXEConfigPrinter
        {
            public static void PrintPXEConfig(PXEConfig config)
            {
                Console.WriteLine("TestDHCP - Configuration:");
                Console.WriteLine($"BindAddress: {config.BindAddress}");
                Console.WriteLine($"NetMask: {config.NetMask}");
                Console.WriteLine($"Verbose: {config.Verbose}");
                Console.WriteLine($"DHCPPort: {config.DHCPPort}");
                Console.WriteLine($"HTTPPort: {config.HTTPPort}");
                Console.WriteLine($"TFTPPort: {config.TFTPPort}");
                Console.WriteLine($"ServerDirectory: {config.ServerDirectory}");
                Console.WriteLine($"Loader: {config.Loader}");
                Console.WriteLine($"TFTPWindowSize: {config.TFTPWindowSize}");
                Console.WriteLine($"");
            }
        }

        #endregion
    }

    public enum Loader
    {
        IPXE,
        MICROSOFT,
        SHIM_GRUB2,
        SYSLINUX,
        UEFI_HTTP
    }
}
