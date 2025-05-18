using System;
using System.Threading;
using KnightServer; // 使用正确的命名空间

namespace SoulKnight.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            KnightServer.Server server = new KnightServer.Server(9999);
            Console.WriteLine("服务器启动成功...");
            
            // 检查是否有命令行参数指示以服务模式运行
            if (args.Length > 0 && args[0] == "--service")
            {
                // 使用 ManualResetEvent 让程序保持运行
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                waitHandle.WaitOne();
            }
            else
            {
                // 交互模式，等待按键退出
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }
    }
}
