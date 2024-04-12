using System.Text;

namespace SickRfid;

internal static class Commands
{
    internal const string START = "\u0002sMN MIStartIn\u0003";
    internal const string STOP = "\u0002sMN MIStopIn\u0003";
    
    internal static byte[] START_REQUEST_DATA = Encoding.ASCII.GetBytes(START);
    internal static byte[] STOP_REQUEST_DATA = Encoding.ASCII.GetBytes(STOP);
}

internal static class Acknowledgements
{
    internal const string ACK_START = "\u0002sAN MIStartIn\u0003";
    internal const string ACK_STOP = "\u0002sAN MIStopIn\u0003";
}