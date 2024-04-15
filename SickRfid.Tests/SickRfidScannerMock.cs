using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SickRfid.Tests;

public sealed class SickRfidScannerMock : IDisposable
{
    private record StateObject
    {
        internal const int BufferSize = 1024;
        internal readonly byte[] Buffer = new byte[BufferSize];
        internal readonly StringBuilder Sb = new();
        internal Socket? WorkSocket;
    }
    
    private readonly Socket _listener;
    private bool _disposed;
    private static readonly ManualResetEvent AllDone = new(false);
    private const char CommandEnd = '\u0003';
    private readonly object _lockObjectRead = new();
    private readonly List<Socket> _clients = new();

    public SickRfidScannerMock()
    {
        var localEndPoint = new IPEndPoint(IPAddress.Any, 2112);

        // Create a TCP/IP socket.  
        _listener = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        _listener.Bind(localEndPoint);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(StartListener, cancellationToken);

        return Task.CompletedTask;
    }

    private void StartListener()
    {
        // Bind the socket to the local endpoint and listen for incoming connections.  
        _listener.Listen(10);
        try
        {
            while (true)
            {
                AllDone.Reset();
                // Accept incoming connection, invoke the AcceptCallback method
                _listener.BeginAccept(AcceptCallback, _listener);
                AllDone.WaitOne();
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"SocketException: {e}");
        }
        finally
        {
            _listener.Dispose();
        }
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        AllDone.Set();

        // Get the socket that handles the client request.  
        var listener = (Socket?)ar.AsyncState;
        Socket? handler;
        try
        {
            handler = listener?.EndAccept(ar);
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (handler is null) return;

        lock (_clients)
        {
            _clients.Add(handler);
        }
        // Create the state object.  
        var state = new StateObject { WorkSocket = handler };

        // Begin receiving, then invoke the ReadCallBack method
        handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
    }

    private void ReadCallback(IAsyncResult ar)
    {
        // Retrieve the state object and the handler socket from the asynchronous state object.  
        var state = (StateObject?)ar.AsyncState;
        var handler = state?.WorkSocket;

        // Read data from the client socket.
        if (handler is null) return;
        int bytesRead;
        try
        {
            bytesRead = handler.EndReceive(ar);
        }
        catch (SocketException ex)
        {
            Console.WriteLine(ex);
            return;
        }

        if (bytesRead <= 0) return;
        // There might be more data, so store the data received so far.  
        state?.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

        // Check for carriage return character. If it is not there, read more data.  
        var content = state?.Sb.ToString();
        if (content?.IndexOf(CommandEnd, StringComparison.Ordinal) > -1)
        {
            var response = HandleRequest(content);

            // Send the response to the client
            Send(handler, response);
        }
        else
        {
            // Not all data received. Get more by re-invoking this method.
            if (state?.Buffer is not null)
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
        }
    }

    private string HandleRequest(string requestStr)
    {
        // Check if request is valid:
        if (string.IsNullOrEmpty(requestStr)) return "BAD_COMMAND";
        lock (_lockObjectRead)
        {
            // Check command:
            return requestStr switch
            {
                Commands.START => Acknowledgements.ACK_START,
                Commands.STOP => Acknowledgements.ACK_STOP,
                _ => "BAD_COMMAND"
            };
        }
    }

    public Task ScanBarcode(string barcode)
    {
        var data = Encoding.ASCII.GetBytes(barcode);
        var tasks = new List<Task>();
        lock (_clients)
        {
            tasks.AddRange(_clients.Select(client => client.SendAsync(data, SocketFlags.None)));
        }
        return Task.WhenAll(tasks);
    }

    private void Send(Socket handler, string data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        var byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device and invoke the SendCallback method.
        handler.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, handler);
    }

    private void SendCallback(IAsyncResult ar)
    {
        if (ar.AsyncState is null) return;
        try
        {
            // Retrieve the socket from the state object.  
            var handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            handler.EndSend(ar);

            // Create a new state object to prepare for the next request.  
            var state = new StateObject { WorkSocket = handler };
            if (handler.Connected)
            {
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }


    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _listener.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException)
        {
            // ignore            
        }

        _listener.Dispose();
        _disposed = true;
    }
}