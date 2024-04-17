using System.Text;

namespace SickRfid;

/// <summary>
///     Contains the commands that can be sent to the Sick RFID reader.
/// </summary>
internal static class Commands
{
    /// <summary>
    ///     The command to start scanning for RFID tags.
    /// </summary>
    internal const string START = "\u0002sMN MIStartIn\u0003";
    
    /// <summary>
    ///     The command to stop scanning for RFID tags.
    /// </summary>
    internal const string STOP = "\u0002sMN MIStopIn\u0003";
    
    /// <summary>
    ///     The data to send to the Sick RFID reader to start scanning for RFID tags.
    /// </summary>
    internal static byte[] START_REQUEST_DATA = Encoding.ASCII.GetBytes(START);
    
    /// <summary>
    ///     The data to send to the Sick RFID reader to stop scanning for RFID tags.
    /// </summary>
    internal static byte[] STOP_REQUEST_DATA = Encoding.ASCII.GetBytes(STOP);
}
