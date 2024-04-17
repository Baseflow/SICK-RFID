using System.Net;

namespace SickRfid;

/// <summary>
///     A builder that can be used to create a disconnected SickRfidController.
///     The disconnected SickRfidController can be used to connect to the Sick RFID reader.
/// </summary>
public class SickRfidControllerBuilder
{
    private readonly IPAddress _ipAddress;
    private int _port = 2112;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SickRfidControllerBuilder"/> class.
    /// </summary>
    /// <param name="ipAddress">
    ///     The IP address of the Sick RFID reader.
    /// </param>
    public SickRfidControllerBuilder(IPAddress ipAddress)
    {
        _ipAddress = ipAddress;
    }

    /// <summary>
    ///     Sets the port of the Sick RFID reader.
    /// </summary>
    /// <param name="port">
    ///     The port of the Sick RFID reader.
    /// </param>
    /// <returns>
    ///     The <see cref="SickRfidControllerBuilder"/> instance, with the port set.
    /// </returns>
    public SickRfidControllerBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    /// <summary>
    ///     Builds a disconnected SickRfidController.z
    /// </summary>
    /// <returns>
    ///     A new instance of a disconnected SickRfidController.
    /// </returns>
    public DisconnectedSickRfidController Build()
    {
        return new DisconnectedSickRfidController(_ipAddress, _port);
    }
}