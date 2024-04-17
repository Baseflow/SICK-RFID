using System.Net;

namespace SickRfid;

public class SickRfidControllerBuilder
{
    private readonly IPAddress _ipAddress;
    private int _port = 2112;

    public SickRfidControllerBuilder(IPAddress ipAddress)
    {
        _ipAddress = ipAddress;
    }

    public SickRfidControllerBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public DisconnectedSickRfidController Build()
    {
        return new DisconnectedSickRfidController(_ipAddress, _port);
    }
}