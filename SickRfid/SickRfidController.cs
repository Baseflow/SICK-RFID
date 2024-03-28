using System.Net;
using System.Net.Sockets;

namespace SickRfid;

public record Disconnected;
public record Connected;

public abstract class SickRfidControllerState<T>
{
}

public class DisconnectedSickRfidController: SickRfidControllerState<Disconnected>
{
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    
    internal DisconnectedSickRfidController(IPAddress ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public async Task<ConnectedSickRfidController> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(_ipAddress, _port, cancellationToken).ConfigureAwait(true);
        return new ConnectedSickRfidController(socket);
    }
}

public class ConnectedSickRfidController: SickRfidControllerState<Connected>, IDisposable
{
    private readonly Socket _socket;
    
    internal ConnectedSickRfidController(Socket socket)
    {
        _socket = socket;
    }
    
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _socket.DisconnectAsync(false, cancellationToken).ConfigureAwait(true);
        _socket.Close();
        _socket.Dispose();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _socket.SendAsync(Commands.START_REQUEST_DATA, SocketFlags.None, cancellationToken).ConfigureAwait(true);
    }
    
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _socket.SendAsync(Commands.STOP_REQUEST_DATA, SocketFlags.None, cancellationToken).ConfigureAwait(true);
    }

    public void Dispose()
    {
        if (_socket == null)
        {
            return;
        }
        _socket.Disconnect(true);
        _socket.Close();
        _socket.Dispose();
    }
}

// Create a builder pattern for the SickRfidController class
// It should at least have parameters for the IP address and port of the RFID reader
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