using System.Data;
using System.Net.Sockets;
using System.Text;

namespace SickRfid;

public sealed class ConnectedSickRfidController : SickRfidControllerState, IDisposable
{
    private Socket? _socket;

    internal ConnectedSickRfidController SetSocket(Socket socket)
    {
        if (_socket is not null)
        {
            throw new ConstraintException("Cannot assign a socket twice");
        }
        _socket = socket;
        return this;
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_socket is null) throw new ConstraintException("Socket is not connected");
        await _socket.DisconnectAsync(false, cancellationToken).ConfigureAwait(true);
        _socket.Close();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {        
        if (_socket is null) throw new ConstraintException("Socket is not connected");
        await _socket.SendAsync(Commands.START_REQUEST_DATA, SocketFlags.None, cancellationToken).ConfigureAwait(true);
    }

    public async Task<string> ListenAsync(CancellationToken cancellationToken = default)
    {
        if (_socket is null) throw new ConstraintException("Socket is not connected");
        var buffer = new byte[1024];

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("No message received within timeout");
            } 
            while (_socket.Available > 0)
            {
                var result = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                if (result <= 0) continue;
                var message = Encoding.ASCII.GetString(buffer, 0, result);

                
                if (string.IsNullOrEmpty(message) || 
                    message.Equals(Acknowledgements.ACK_START, StringComparison.Ordinal) ||
                    message.Contains(Acknowledgements.ACK_STOP, StringComparison.Ordinal)) continue;
                
                return message;
            }
        }
    }

    public async Task<string> ScanRfidAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await StartAsync(cancellationToken).ConfigureAwait(false);
            return await ListenAsync(cancellationToken).ConfigureAwait(false);
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
                await StopAsync(ctx.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to close socket within reasonable timeframe: {e}");
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_socket is null) throw new ConstraintException("Socket is not connected");
        await _socket.SendAsync(Commands.STOP_REQUEST_DATA, SocketFlags.None, cancellationToken).ConfigureAwait(true);
    }

    public void Dispose()
    {
        _socket?.Disconnect(true);
        _socket?.Close();
#pragma warning disable IDISP007
        _socket?.Dispose();
#pragma warning restore IDISP007
    }
}