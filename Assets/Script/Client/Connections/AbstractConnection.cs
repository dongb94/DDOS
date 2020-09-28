
using System.Net.Sockets;
using System.Threading.Tasks;

public abstract class AbstractConnection
{
    protected static string IP = "13.125.85.119";
    protected static int PORT = 3000;
    
    protected const int MAX_BUFFER_SIZE = ushort.MaxValue;
    protected const int RESERVE_SIZE = ushort.MaxValue;
    
    protected byte[] _sendBuffer;
    protected byte[] _rcvBuffer;
    
    public static void SetTarget(string ip, uint port)
    {
        IP = ip;
        PORT = (int) port;
    }

    public virtual void Initialize()
    {
        _rcvBuffer = new byte[MAX_BUFFER_SIZE];
        _sendBuffer = new byte[MAX_BUFFER_SIZE];
    }
    public abstract Task<Socket> Connect(uint sendCount, uint bufferSize);
    public abstract void Close();
}