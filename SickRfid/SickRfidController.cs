using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SickRfid;

public record Disconnected;

public record Connected;

public abstract class SickRfidControllerState<T>
{
}

public class DisconnectedSickRfidController : SickRfidControllerState<Disconnected>
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

public class ConnectedSickRfidController : SickRfidControllerState<Connected>, IDisposable
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

    public async Task<string> ListenAsync(CancellationToken cancellationToken = default)
    {
        var buffer = new byte[1024];

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("No message received within timeout");
            } 
            while (_socket.Available > 0)
            {
                var result = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
                if (result <= 0) continue;
                var message = Encoding.ASCII.GetString(buffer, 0, result);

                
                if (string.IsNullOrEmpty(message) || 
                    message.Equals(Acknowledgements.ACK_START) ||
                    message.Contains(Acknowledgements.ACK_STOP)) continue;
                
                return message;
            }
        }
    }

    public async Task<string> ScanRfidAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await StartAsync(cancellationToken).ConfigureAwait(false);
            return await ListenAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to read barcode: {e}");
            throw;
        }
        finally
        {
            try
            {
                using var ctx = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                await StopAsync(ctx.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to close socket within reasonable timeframe: {e}");
            }
        }

        return string.Empty;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _socket.SendAsync(Commands.STOP_REQUEST_DATA, SocketFlags.None, cancellationToken).ConfigureAwait(true);
    }

    public void Dispose()
    {
        _socket.Disconnect(true);
        _socket.Close();
        _socket.Dispose();
    }
}

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