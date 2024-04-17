using System.Net;
using System.Net.Sockets;

namespace SickRfid;

/// <summary>
///     Represents the disconnected state of the Sick RFID controller.
///     This state can be used to connect to the Sick RFID reader. It cannot be instantiated directly, but is returned by the <see cref="SickRfidControllerBuilder"/>.
/// </summary>
public sealed class DisconnectedSickRfidController : SickRfidControllerState
{
    private readonly IPAddress _ipAddress;
    private readonly int _port;

    internal DisconnectedSickRfidController(IPAddress ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public async Task<ConnectedSickRfidController> ConnectAsync()
    {
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(_ipAddress, _port).ConfigureAwait(true);
        return new ConnectedSickRfidController().SetSocket(socket);
    }
}