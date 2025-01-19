using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    class Program
    {
        private static Socket listenSocket;

        static void Main(string[] args)
        {
            ListenSocket();
        }

        /// <summary>
        /// 启用客户端连接监听
        /// </summary>
        private static void ListenSocket()
        {
            //实例化socket
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //向操作系统申请一个可用的ip和端口用来通讯
            listenSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1011));

            //设置最多3000个排队连接请求
            listenSocket.Listen(3000);
            Console.WriteLine("启动监听{0}成功", listenSocket.LocalEndPoint?.ToString());

            //开启一个线程用来监听客户端连接
            Thread mThread = new Thread(ListenClientCallBack);
            mThread.Start();
        }

        /// <summary>
        /// 监听客户端连接
        /// </summary>
        private static void ListenClientCallBack()
        {
            while (true)
            {
                //接收客户端请求
                Socket connectionSocket = listenSocket.Accept();

                //创建一个客户端Socket来负责与该客户端通信
                Connection connection = new Connection(connectionSocket);

                Console.WriteLine("客户端{0}成功连接", connectionSocket.RemoteEndPoint?.ToString());
            }
        }
    }
}