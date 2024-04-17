using System.Data;
using System.Net.Sockets;
using System.Text;

namespace SickRfid;

/// <summary>
///     Represents the connected state of the Sick RFID controller.
///     This state can be used to send commands to and read from the Sick RFID reader.
/// </summary>
public sealed class ConnectedSickRfidController : SickRfidControllerState, IDisposable
{
    private Socket? _socket;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConnectedSickRfidController"/> class.
    ///     This class cannot be instantiated directly, but is returned by the <see cref="DisconnectedSickRfidController"/>.
    /// </summary>
    /// <param name="socket">
    ///     The socket that is connected to the Sick RFID reader.
    /// </param>
    /// <returns>
    ///     A new instance of a connected SickRfidController.
    /// </returns>
    /// <exception cref="ConstraintException">
    ///     Thrown when the socket is already assigned.
    /// </exception>
    internal ConnectedSickRfidController SetSocket(Socket socket)
    {
        if (_socket is not null)
        {
            throw new ConstraintException("Cannot assign a socket twice");
        }
        _socket = socket;
        return this;
    }

    /// <summary>
    ///     Closes the connection to the Sick RFID reader.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The optional cancellation token to cancel the operation.
    /// </param>
    /// <exception cref="ConstraintException">
    ///     Thrown when the socket is not connected.
    /// </exception>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_socket is null) throw new ConstraintException("Socket is not connected");
        await _socket.DisconnectAsync(false, cancellationToken).ConfigureAwait(true);
        _socket.Close();
    }

    /// <summary>
    ///     Sends a command to the Sick RFID reader to start scanning for RFID tags.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The optional cancellation token to cancel the operation.
    /// </param>
    /// <exception cref="ConstraintException">
    ///     Thrown when the socket is not connected.
    /// </exception>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {        
        if (_socket is null) throw new ConstraintException("Socket is not connected");
        await _socket.SendAsync(Commands.START_REQUEST_DATA, SocketFlags.None, cancellationToken).ConfigureAwait(true);
    }

    
    /// <summary>
    ///     Listens for a message from the Sick RFID reader.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The optional cancellation token to cancel the operation.
    /// </param>
    /// <returns>
    ///     The message received from the Sick RFID reader.
    /// </returns>
    /// <exception cref="ConstraintException">
    ///     Thrown when the socket is not connected.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when no message is received within the timeout.
    /// </exception>
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

    
    /// <summary>
    ///     Scans a single RFID tag, by combining the start, listen, and stop methods.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The optional cancellation token to cancel the operation.
    /// </param>
    /// <returns>
    ///     The RFID tag that was scanned.
    /// </returns>
    public async Task<string> ScanRfidAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await StartAsync(cancellationToken).ConfigureAwait(false);
            return await ListenAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to read RFID tag: {e}");
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

    /// <summary>
    ///     Sends a command to the Sick RFID reader to stop scanning for RFID tags.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The optional cancellation token to cancel the operation.
    /// </param>
    /// <exception cref="ConstraintException">
    ///     Thrown when the socket is not connected.
    /// </exception>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_socket is null) throw new ConstraintException("Socket is not connected");
        await _socket.SendAsync(Commands.STOP_REQUEST_DATA, SocketFlags.None, cancellationToken).ConfigureAwait(true);
    }

    /// <summary>
    ///     Closes the connection to the Sick RFID reader and disposes of the socket.
    /// </summary>
    public void Dispose()
    {
        _socket?.Disconnect(true);
        _socket?.Close();
#pragma warning disable IDISP007
        _socket?.Dispose();
#pragma warning restore IDISP007
    }
}