using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RemoteControl : MonoBehaviour
{
    private Socket _socket;
    private List<ClientObjectUI> _clients;
    private bool _isListen;

    private byte[] _sendBuffer;

    public Commend commend;
    public string host;
    public string port;
    public string clientNum;
    public string packetNum;
    public string packetSize;

    private RemoteControlUI _remoteUI;

    public enum Commend
    {
        connect,
        disconnect,
        quit,
    }

    private void Awake()
    {
        _remoteUI = gameObject.AddComponent<RemoteControlUI>();
        
        _clients = new List<ClientObjectUI>();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _isListen = false;
        
        _sendBuffer = new byte[512];
        
        IPAddress hostIP = IPAddress.Parse("203.237.125.89");
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 2080);
        _socket.Bind(ep);
        
        _socket.Listen(128);
    }

    private async void Update()
    {
        if (_isListen) return;
        Task.Run(ClientConnect);
    }

    private async void ClientConnect()
    {
        _isListen = true;
        var connection = await _socket.AcceptAsync();

        _remoteUI.readyQueue.Enqueue(connection);
        
        _isListen = false;
    }

    public void ConnectCommend()
    {
        var commend = $"{this.commend},{host},{port},{clientNum},{packetSize},{packetNum}";

        _sendBuffer = Encoding.UTF8.GetBytes(commend);
        
        for (int i = 0; i < _clients.Count; i++)
        {
            if(!_clients[i].toggle.isOn) continue;
            
            try
            {
                _clients[i].socket.Send(_sendBuffer, 0, _sendBuffer.Length, SocketFlags.None);
            }
            catch (Exception e)
            {
                LogText.Instance.Print(i + "--" + e.ToString());
                RemoveClient(_clients[i]);
                i--;
            }
        }
    }

    public void AddClient(ClientObjectUI client)
    {
        _clients.Add(client);
    }

    public void RemoveClient(ClientObjectUI client)
    {
        _clients.Remove(client);
        _remoteUI.RemoveClient(client);
        
        try
        {
            if (client.socket.Connected)
            {
                client.socket.Shutdown(SocketShutdown.Both);
                client.socket.Disconnect(false);
            }
        }
        catch (Exception e)
        {
            LogText.Instance.Print(e);
        }
        finally
        {
            client.socket.Close();
        }
    }
    
    private void OnDestroy()
    {
        LogText.Instance.Destory();
    }
}
