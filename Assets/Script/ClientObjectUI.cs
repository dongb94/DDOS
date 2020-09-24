
using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ClientObjectUI : MonoBehaviour
{
    public Socket socket;
    public Text localIp, remoteIp, connection;

    public void Init()
    {
        localIp = transform.Find("LocalIP/Text").GetComponent<Text>();
        remoteIp = transform.Find("RemoteIP/Text").GetComponent<Text>();
        connection = transform.Find("Connection/Text").GetComponent<Text>();
    }
}