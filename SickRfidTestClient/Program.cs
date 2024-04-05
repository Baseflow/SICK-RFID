// See https://aka.ms/new-console-template for more information

using System.Net;
using SickRfid;

Console.WriteLine("Hello, World!");
var controller = new SickRfidControllerBuilder(new IPAddress(new byte[] { 192, 168, 0, 149 }))
    .WithPort(2112)
    .Build();

    
    var controller12 = new SickRfidControllerBuilder(IPAddress.Parse("192.168.0.149")).Build();