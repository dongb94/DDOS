
using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ClientObjectUI : MonoBehaviour
{
    public Socket socket;
    public Toggle toggle;
    public Text localIp, remoteIp, connection;

    public void Init()
    {
        toggle = transform.Find("Toggle").GetComponent<Toggle>();
        localIp = transform.Find("LocalIP/Text").GetComponent<Text>();
        remoteIp = transform.Find("RemoteIP/Text").GetComponent<Text>();
        connection = transform.Find("Connection/Text").GetComponent<Text>();
    }
}