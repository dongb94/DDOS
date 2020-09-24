using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.Win32;

public class Ready : MonoBehaviour
{
    private static readonly int BUFFERSIZE = 512;
    
    private MultiConnect _connection;
    private Socket _socket;

    private byte[] _recvBuffer;
    
    void Awake()
    {
        if (!IsAdministrator())
        {
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.UseShellExecute = true;
            procInfo.FileName = Environment.GetCommandLineArgs()[0]; // 실행 환경중 args[0]은 자기 자신 (에디터에서 실행 시키면 유니티 에디터를 반환한다.)
            procInfo.WorkingDirectory = Environment.CurrentDirectory;
            procInfo.Verb = "runas";
        
            Process.Start(procInfo);
            
            Application.Quit();
        }
        
        Application.runInBackground = true;
        _connection = gameObject.AddComponent<MultiConnect>();
        _recvBuffer = new byte[BUFFERSIZE];

        try
        {
            var regiPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey registry = Registry.LocalMachine.OpenSubKey(regiPath);

            if (registry.GetValue("DG_TCPClient") == null)
            {
                registry.Close();
                registry = Registry.LocalMachine.OpenSubKey(regiPath, true);
                registry.SetValue("DG_TCPClient", Environment.GetCommandLineArgs()[0]);
                registry.Close();
            }
        }
        catch (Exception e)
        {
            LogText.Instance.Print(e.ToString());
        }

        Task.Run(ReadyToListen);
    }

    public void ReadyToListen()
    {
        string msg;
        
        connect:
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SendBufferSize = _socket.ReceiveBufferSize = BUFFERSIZE;
            _socket.NoDelay = true;
            _socket.ReceiveTimeout = 10000;

            _socket.Connect("203.237.125.89", 2080);

            receive:
            try
            {
                while (true)
                {
                    _socket.Receive(_recvBuffer, 0, BUFFERSIZE, SocketFlags.None);

                    msg = Encoding.Default.GetString(_recvBuffer);

                    var msgParse = msg.Split('.');

                    if (msgParse[0].ToLower().Equals("quit"))
                    {
                        Application.Quit();
                    }
                    else if (msgParse[0].ToLower().Equals("disconnect"))
                    {
                        _connection.DisConnectAll();
                        goto receive;
                    }
                    
                    var clientNum = uint.Parse(msgParse[0]);
                    var packetSize = uint.Parse(msgParse[1]);
                    var packetNum = uint.Parse(msgParse[2]);
                    
                    _connection.DoConnect(clientNum, packetSize, packetNum);
                }
            }
            catch (SocketException se)
            {
                LogText.Instance.Print("recv err : "+se.ToString());
                if (se.SocketErrorCode == SocketError.TimedOut)
                {
                    goto receive;
                }
                else
                {
                    throw;
                }
            }
        }
        catch (Exception e)
        {
            LogText.Instance.Print(e.ToString());

            try
            {
                if (_socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Disconnect(false);
                }

                _connection.DisConnectAll();
            }
            catch (Exception ee)
            {
                LogText.Instance.Print(ee.ToString());
            }
            finally
            {
                _socket.Close();
            }

            goto connect;
        }
    }

    public bool IsAdministrator()
    {
        WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private void OnDestroy()
    {
        LogText.Instance.Destory();
    }
}