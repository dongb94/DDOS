
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class MultiConnect : MonoBehaviour
{
    private List<AbstractConnection> _clients;

    private void Awake()
    {
        _clients = new List<AbstractConnection>();
    }

    public void DoConnect(string host, uint port, uint clientNum, uint packetSize, uint sendPacketNum)
    {
        if (_clients.Count < clientNum)
        {
            while (_clients.Count != clientNum)
            {
                var newSocket = new NodeConnect(); // 이 부분을 바꿔서 접속 방식 변경
                _clients.Add(newSocket);
                newSocket.Initialize();
            }
        }

        try
        {
            AbstractConnection.SetTarget(host, port);
            for (int i = 0; i < clientNum; i++)
            {
                _clients[i].Connect(sendPacketNum, packetSize);
            }
        }
        catch (ArgumentOutOfRangeException e)
        {
            LogText.Instance.Print("=== Argument Out Of Range : " + e);
        }
        catch (Exception e)
        {
            LogText.Instance.Print("Do Connect Err : " + e);
        }
    }

    public void DisConnectAll()
    {
        for (int i = 0; i < _clients.Count; i++)
        {
            _clients[i].Close();
        }
    }
}