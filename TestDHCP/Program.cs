using TestDHCP;

var config = PXEConfig.Load();

PXEConfig.PXEConfigPrinter.PrintPXEConfig(config);

var pxe_server = new PXEServer(config);
pxe_server.Start();


Console.WriteLine("Press ENTER to exit");
Console.ReadLine();

pxe_server.Stop();
