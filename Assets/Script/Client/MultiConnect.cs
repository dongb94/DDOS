
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class MultiConnect : MonoBehaviour
{
    private List<NodeConnect> _clients;

    private void Awake()
    {
        _clients = new List<NodeConnect>();
    }

    public void DoConnect(uint clientNum, uint packetSize, uint sendPacketNum)
    {
        if (_clients.Count < clientNum)
        {
            while (_clients.Count != clientNum)
            {
                var newSocket = new NodeConnect();
                _clients.Add(newSocket);
                newSocket.Initialize();
            }
        }
        
        try
        {
            for (int i = 0; i < clientNum; i++)
            {
                _clients[i].Connect(sendPacketNum, packetSize);
            }
        }
        catch (ArgumentOutOfRangeException e)
        {
            LogText.Instance.Print("Do Connect Err : "+e);
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