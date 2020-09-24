using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class RemoteControlUI :MonoBehaviour
{
    public Queue<Socket> readyQueue;
    private Queue<ClientObjectUI> _clientUiPool;
    
    private RemoteControl remote;

    private Transform _contents;
    private GameObject _clientPrefab;
    
    private void Awake()
    {
        remote = transform.GetComponent<RemoteControl>();
        
        transform.Find("ClientNum").GetComponent<InputField>().onValueChanged.AddListener(OnChangeClientNum);
        transform.Find("PacketSize").GetComponent<InputField>().onValueChanged.AddListener(OnChangePacketSize);
        transform.Find("PacketNum").GetComponent<InputField>().onValueChanged.AddListener(OnChangePacketNum);
        transform.Find("Connect").GetComponent<Button>().onClick.AddListener(OnClickConnectButton);

        _contents = transform.Find("Scroll View/Viewport/Content");
        _clientPrefab = _contents.Find("Client").gameObject;

        readyQueue = new Queue<Socket>();
        _clientUiPool = new Queue<ClientObjectUI>();
    }

    private void Update()
    {
        if (readyQueue.Count > 0)
        {
            AddClient();
        }
    }

    public void AddClient()
    {
        ClientObjectUI client;
        if (_clientUiPool.Count > 0)
        {
            client = _clientUiPool.Dequeue();
        }
        else
        {
            client = Instantiate(_clientPrefab, _contents).AddComponent<ClientObjectUI>();
        }

        client.socket = readyQueue.Dequeue();
        client.Init();
        client.localIp.text = client.socket.LocalEndPoint.ToString();
        client.remoteIp.text = client.socket.RemoteEndPoint.ToString();
        client.connection.text = "0";
        client.gameObject.SetActive(true);
        remote.AddClient(client);
    }

    public void RemoveClient(ClientObjectUI client)
    {
        client.gameObject.SetActive(false);
        _clientUiPool.Enqueue(client);
    }

    private void OnChangeClientNum(string num)
    {
        remote.clientNum = num;
    }
    
    private void OnChangePacketSize(string size)
    {
        remote.packetSize = size;
    }
    
    private void OnChangePacketNum(string num)
    {
        remote.packetNum = num;
    }

    private void OnClickConnectButton()
    {
        remote.ConnectCommend();
    }
    
    
}
