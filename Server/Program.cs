using System;
using KnightServer; // 使用正确的命名空间

namespace SoulKnight.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            KnightServer.Server server = new KnightServer.Server(9999);
            Console.WriteLine("服务器启动成功...");
            Console.ReadKey();
        }
    }
}
