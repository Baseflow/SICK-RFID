namespace SickRfid;

/// <summary>
///     Contains the acknowledgements that can be received from the Sick RFID reader.
/// </summary>
internal static class Acknowledgements
{
    /// <summary>
    ///     The acknowledgement that the Sick RFID reader sends when it starts scanning for RFID tags.
    /// </summary>
    internal const string ACK_START = "\u0002sAN MIStartIn\u0003";
    
    /// <summary>
    ///     The acknowledgement that the Sick RFID reader sends when it stops scanning for RFID tags.
    /// </summary>
    internal const string ACK_STOP = "\u0002sAN MIStopIn\u0003";
}
