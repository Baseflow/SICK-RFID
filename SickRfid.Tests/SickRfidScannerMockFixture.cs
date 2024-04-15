namespace SickRfid.Tests;

public class SickRfidScannerMockFixture: IDisposable
{
    internal SickRfidScannerMock Mock { get; private set; }
    
    public SickRfidScannerMockFixture()
    {
        Mock = new SickRfidScannerMock();
        _ = Mock.StartAsync(CancellationToken.None);
    }

    public void Dispose()
    {
        Mock.Dispose();
    }
}