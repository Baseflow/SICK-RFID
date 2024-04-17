using System.Net;
using Xunit.Abstractions;

namespace SickRfid.Tests;

using Xunit;

public sealed class Test :  IClassFixture<SickRfidScannerMockFixture>, IAsyncDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly SickRfidScannerMock _rfidScanner;

    private const string IpAddress = "127.0.0.1";
    private const int Port = 2112;
    
    private const string Barcode = "E8E0FE73-4CC0-4710-BF57-941EA523F904";
    
    public Test(ITestOutputHelper testOutputHelper, SickRfidScannerMockFixture mockFixture)
    {
        _testOutputHelper = testOutputHelper;
        _rfidScanner = mockFixture.Mock;
    }
    
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
        var controller = new SickRfidControllerBuilder(IPAddress.Parse(IpAddress))
            .WithPort(Port)
            .Build();
#pragma warning disable REFL039
        Assert.Equal(typeof(DisconnectedSickRfidController), controller.GetType());

        using var connectedController = await controller.ConnectAsync();

        Assert.Equal(typeof(ConnectedSickRfidController), connectedController.GetType());
#pragma warning restore REFL039
        
        await connectedController.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));
        await connectedController.StopAsync();
    }
    
    [Fact]
    public async Task TestListenAsync()
    {
        var controller = new SickRfidControllerBuilder(IPAddress.Parse(IpAddress))
            .WithPort(Port)
            .Build();
#pragma warning disable REFL039
        Assert.Equal(typeof(DisconnectedSickRfidController), controller.GetType());
        
        using var connectedController = await controller.ConnectAsync();

        Assert.Equal(typeof(ConnectedSickRfidController), connectedController.GetType());
#pragma warning restore REFL039
      
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            _ = ReadBarcodeWithDelay(TimeSpan.FromSeconds(2), cts.Token);
            var message = await connectedController.ScanRfidAsync(cts.Token);
            _testOutputHelper.WriteLine(message);
            Assert.Equal(Barcode, message);
        }
        catch (OperationCanceledException)
        {
            _testOutputHelper.WriteLine("Did not receive a barcode within 10 seconds....");
            throw;
        }
    }

    // Sets a delay before reading barcode with the mock.
    private Task<Task> ReadBarcodeWithDelay(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(async () =>
        {
            await Task.Delay(delay, CancellationToken.None).ConfigureAwait(false);
            _testOutputHelper.WriteLine("Sending barcode");
            try
            {
                await _rfidScanner.ScanRfidAsync(Barcode).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
            }
        }, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        // Allow all communication to shutdown in a orderly fashion.
        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        _rfidScanner.Dispose();
    }
}