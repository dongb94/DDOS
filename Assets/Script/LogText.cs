
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class LogText
{
    private const uint LOG_FILE_SIZE = 65535;
    
    private static LogText _instance;
    private static uint _logStack;
    private static uint _logFileNum;
    
    private FileStream file;

    public static LogText Instance => _instance ?? (_instance = new LogText());

    public LogText()
    {
        _logFileNum = 0;
        _logStack = 0;
        try
        {
            while(!OpenNextLogFile());
            
            Print("\n----------------------\n"+DateTime.Now);
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.Log(e);
            throw;
#endif
        }
    }

    ~LogText()
    {
        Destory();
    }
    
    public void Print(string err)
    {
        err = $"{_logStack++}:\t{err}\n";
        var buffer = Encoding.UTF8.GetBytes(err);
        file.Write(buffer, 0, buffer.Length);

        if (CheckFileSize())
        {
            file.Flush();
            file.Close();
            OpenNextLogFile();
        }
    }
    
    public void Print(object err)
    {
        Print(err.ToString());
    }

    public void Destory()
    {
        file?.Flush();
        file?.Close();
        _instance = null;
    }

    private bool OpenNextLogFile()
    {
        _logFileNum++;
        var path = Environment.CurrentDirectory + $"\\log{_logFileNum}.txt";

        try
        {
            file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (CheckFileSize())
            {
                file.Close();
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        file.Position = file.Length;

        return true;
    }

    private bool CheckFileSize()
    {
        return file.Length > LOG_FILE_SIZE;
    }
}