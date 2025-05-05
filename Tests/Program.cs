using System;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Soul Knight 服务器测试程序");
            Console.WriteLine("==========================");
            
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "functional":
                        new FunctionalTest().RunAllTests();
                        break;
                    case "stress":
                        int clientCount = args.Length > 1 ? int.Parse(args[1]) : 100;
                        new StressTest().RunTest(clientCount);
                        break;
                    case "stability":
                        int durationMinutes = args.Length > 1 ? int.Parse(args[1]) : 60;
                        new StabilityTest().RunTest(durationMinutes * 60);
                        break;
                    case "response":
                        new ResponseTimeTest().RunTest();
                        break;
                    default:
                        ShowUsage();
                        break;
                }
            }
            else
            {
                // 默认运行功能测试
                new FunctionalTest().RunAllTests();
            }
            
            Console.WriteLine("测试完成，按任意键退出...");
            Console.ReadKey();
        }
        
        private static void ShowUsage()
        {
            Console.WriteLine("用法: dotnet run [测试类型] [参数]");
            Console.WriteLine("测试类型:");
            Console.WriteLine("  functional - 功能测试");
            Console.WriteLine("  stress [客户端数量] - 压力测试");
            Console.WriteLine("  stability [持续时间(分钟)] - 稳定性测试");
            Console.WriteLine("  response - 响应时间测试");
        }
    }
}