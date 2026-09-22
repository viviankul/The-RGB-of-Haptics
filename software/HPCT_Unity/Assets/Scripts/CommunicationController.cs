using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.Linq.Expressions;

public class CommunicationController : MonoBehaviour
{
    private const int listenPort = 11000;
    private const int sendPort = 11000;
    private UdpClient listener;
    private IPEndPoint groupEP;
    private IPAddress espIP = IPAddress.Parse("172.17.54.196");

    public ConcurrentQueue<float> fingerData = new ConcurrentQueue<float>();

    private static CommunicationController _instance;

    public static CommunicationController Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("CommunicationController instance is null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            listener = new UdpClient(listenPort);
            groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            Debug.Log("Starting Communication thread");
            Thread msgThread = new Thread(ReceiveMessages);
            msgThread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError($"SocketException: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
        }
    }

    private void ReceiveMessages()
    {
        try
        {
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                string senderIP = groupEP.Address.ToString();
                ProcessMsg(bytes);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError(e);
        }
    }

    void Update()
    {
        if (FingerTracking.Instance != null && fingerData != null)
        {
            while (fingerData.TryDequeue(out float data))
            {
                FingerTracking.Instance.updatePose(data);
            }
        }
    }

    public void SendMsg(string msg)
    {
        try
        {
            Debug.Log("Sending message: " + msg);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            byte[] sendbuf = Encoding.ASCII.GetBytes(msg);
            IPEndPoint ep = new IPEndPoint(espIP, sendPort);

            s.SendTo(sendbuf, ep);
            s.Close();
        }
        catch (SocketException e)
        {
            Debug.LogError(e);
        }
    }

    public void ProcessMsg(byte[] bytes)
    {
        string message = System.Text.Encoding.UTF8.GetString(bytes);
        float flexvalue = float.Parse(message);
        fingerData.Enqueue(flexvalue);
    }
}
