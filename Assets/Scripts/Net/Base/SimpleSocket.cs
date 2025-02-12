using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;

//这个没有用噢，lockstep框架自带的，先留着，协议相关都走netmanager
public class SimpleSocket {

    private Socket socketClient;

    // Use this for initialization
    public void Init () {
        Console.WriteLine("Hello World!");
        //创建实例
        socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip = IPAddress.Parse("192.168.0.140");
        IPEndPoint point = new IPEndPoint(ip, 2333);
        //进行连接
        socketClient.Connect(point);

        //不停的接收服务器端发送的消息
        Thread thread = new Thread(Recive);
        thread.IsBackground = true;
        thread.Start(socketClient);
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="o"></param>
    static void Recive(object o)
    {
        // var send = o as Socket;
        // //while (true)
        // {
        //     //获取发送过来的消息
        //     byte[] buffer = new byte[1024 * 1024 * 2];
        //     var effective = send.Receive(buffer);
        //     if (effective == 0)
        //     {
        //         //break;
        //     }
        //     var str = Encoding.UTF8.GetString(buffer, 0, effective);
        //     Debug.Log("receive from server: " + str);
        //     BattleUI.m_scServerInfo = str;
        // }
        var socket = o as Socket;
        if (socket == null)
        {
            Debug.LogError("socket链接异常，退出线程");
            return;
        }
    
        byte[] buffer = new byte[4096];
        try {
            while(true) {
                int effective = socket.Receive(buffer);
                if (effective == 0) break; // 服务器关闭连接
                string str = Encoding.UTF8.GetString(buffer, 0, effective);
                // 主线程更新UI
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    BattleUI.m_scServerInfo = str;
                });
            }
        } catch (Exception e) {
            Debug.LogError($"Receive error: {e.Message}");
        } finally {
            socket.Close();
        }
    }

    public void sendBattleRecordToServer(string record)
    {
        var buffter = Encoding.UTF8.GetBytes(record);
        var temp = socketClient.Send(buffter);
        Thread.Sleep(1000);
    }
}