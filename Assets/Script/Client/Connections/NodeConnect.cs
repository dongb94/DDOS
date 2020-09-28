using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NodeConnect : AbstractConnection
{
    private uint sendCount = 0;
    
    private Socket _socket;
    private Thread _recvListen;

    private int recvSize;

    public string msg;

    public override async Task<Socket> Connect(uint sendCount, uint bufferSize)
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

    public override void Close()
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
            _socket?.Close();
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
                Close();
                break;
            }
            catch (IOException ioex)
            {
                LogText.Instance.Print("WSACancelBlockCall");
                LogText.Instance.Print(ioex);
                Close();
                break;
            }
            catch (SocketException se)
            {
                LogText.Instance.Print("socket exception : "+se);
                Close();
                break;
            }
            catch (Exception ex)
            {
                LogText.Instance.Print("socket receive error : "+ex);
                Close();
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
                    LogText.Instance.Print($"System Exit Error=={e}");
                }
                finally
                {
                    _socket.Close();
                }
            }
        }
    }
}