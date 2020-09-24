using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NodeConnect
{
    private static string IP = "13.125.85.119";
    private static int PORT = 3000;

    private const int MAX_BUFFER_SIZE = 65535;
    private const int RESERVE_SIZE = ushort.MaxValue;

    private uint sendCount = 0;

    private byte[] _sendBuffer;
    private byte[] _rcvBuffer;
    private Socket _socket;

    private Thread _recvListen;

    private int recvSize;

    public string msg;

    public void Initialize()
    {
        _rcvBuffer = new byte[MAX_BUFFER_SIZE];
        _sendBuffer = new byte[MAX_BUFFER_SIZE];
    }

    public static void SetTarget(string ip, uint port)
    {
        IP = ip;
        PORT = (int) port;
    }

    public async Task<Socket> Connect(uint sendCount, uint bufferSize)
    {
        if (_socket != null && _socket.Connected) return _socket;
        
        if (_recvListen!=null)
        {
            _recvListen.Abort();
            _recvListen = null;
        }
        
        try
        {
            this.sendCount = sendCount;
            var sendMsg = $":{bufferSize}";
            for (int i = 0; i < bufferSize; i++)
            {
                sendMsg += "/";
            }
            _sendBuffer = Encoding.UTF8.GetBytes(sendMsg);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SendBufferSize = _socket.ReceiveBufferSize = RESERVE_SIZE;
            _socket.NoDelay = true;
            _socket.ReceiveTimeout = 10000;
            await _socket.ConnectAsync(IP, PORT);

            if (_socket != null)
            {
                if (_recvListen == null)
                {
                    _recvListen = new Thread(DoReceive) {IsBackground = true};
                }

                if (!_recvListen.IsAlive)
                    _recvListen.Start();
            }
        }
        catch (Exception e)
        {
            LogText.Instance.Print("socket connect err : "+e);
            Close();
        }

        return _socket;
    }

    public void Close()
    {
        try
        {
            if (_socket!=null && _socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Disconnect(false);    
            }
        }
        catch (Exception e)
        {
            LogText.Instance.Print(e);
            throw;
        }
        finally
        {
            _socket.Close();
            _socket = null;
        }
    }

    private void DoReceive()
    {
        do
        {
            recvSize = 0;
            try
            {
                if (sendCount > 0)
                {
                    sendCount--;
                    _socket.Send(_sendBuffer, 0, _sendBuffer.Length, SocketFlags.None); ///
                    recvSize = _socket.Receive(_rcvBuffer, 0, MAX_BUFFER_SIZE, SocketFlags.None); ///
                    msg = Encoding.Default.GetString(_rcvBuffer);
                }
            }
            catch (ObjectDisposedException)
            {
                LogText.Instance.Print("tcp close");
                break;
            }
            catch (IOException ioex)
            {
                LogText.Instance.Print("WSACancelBlockCall");
                LogText.Instance.Print(ioex);
                break;
            }
            catch (Exception ex)
            {
                LogText.Instance.Print("socket receive error : "+ex);
                break;
            }

        } while (recvSize > 0);

        // Debug.Log("DataReceiveComplete");
        // Close();
    }

    ~NodeConnect()
    {
        try
        {
            if (_recvListen != null && _recvListen.IsAlive)
            {
                _recvListen.Abort();
            }
        }
        catch (Exception e)
        {
            LogText.Instance.Print(e);
            throw;
        }
        finally
        {
            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Disconnect(false);
                }
                catch (Exception e)
                {
                    LogText.Instance.Print(e);
                    throw;
                }
                finally
                {
                    _socket.Close();
                }
            }
        }
    }
}