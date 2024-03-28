using System.Net;

namespace SickRfid.Tests;

using Xunit;

public class Test
{
    [Fact]
    public void TestStartCommandBytes()
    {
        var startCommand = Commands.START_REQUEST_DATA;
        Assert.Equal(new byte[] { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x4D, 0x49, 0x53, 0x74, 0x61, 0x72, 0x74, 0x49, 0x6E, 0x03 }, startCommand);
    }
    
    [Fact]
    public void TestStopCommandBytes()
    {
        var stopCommand = Commands.STOP_REQUEST_DATA;
        Assert.Equal(new byte[] { 0x02, 0x73, 0x4D, 0x4E, 0x20, 0x4D, 0x49, 0x53, 0x74, 0x6F, 0x70, 0x49, 0x6E, 0x03 }, stopCommand);
    }

    [Fact]
    public async Task ConnectAsync()
    {
        var ipAddress = "192.168.0.149";
        var port = 2112;

        var controller = new SickRfidControllerBuilder(IPAddress.Parse(ipAddress))
            .WithPort(port)
            .Build();
        Assert.Equal(typeof(DisconnectedSickRfidController), controller.GetType());

        using var connectedController = await controller.ConnectAsync();
        Assert.Equal(typeof(ConnectedSickRfidController), connectedController.GetType());
        
        await connectedController.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(2));
        await connectedController.StopAsync();
    }
}