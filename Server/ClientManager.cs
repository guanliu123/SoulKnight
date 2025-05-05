using System;
using System.Collections.Generic;
using System.Linq; // 用于 FirstOrDefault

namespace KnightServer
{
    public class ClientManager
    {
        // 存储所有连接的客户端
        // 考虑线程安全：如果多线程访问，应使用 ConcurrentDictionary 或加锁
        private List<Client> clientList = new List<Client>();
        private readonly object lockObj = new object(); // 用于加锁

        // 单例模式
        private static readonly Lazy<ClientManager> lazyInstance =
            new Lazy<ClientManager>(() => new ClientManager());

        public static ClientManager Instance => lazyInstance.Value;

        // 私有构造函数，防止外部实例化
        private ClientManager() { }

        /// <summary>
        /// 添加一个新客户端
        /// </summary>
        /// <param name="client">要添加的客户端</param>
        public void AddClient(Client client)
        {
            lock (lockObj)
            {
                if (client != null && !clientList.Contains(client))
                {
                    clientList.Add(client);
                    Console.WriteLine($"客户端管理器: 添加了客户端 (ID: {client.Id}, User: {client.userName ?? "N/A"})，当前总数: {clientList.Count}");
                }
            }
        }

        /// <summary>
        /// 移除一个客户端
        /// </summary>
        /// <param name="client">要移除的客户端</param>
        public void RemoveClient(Client client)
        {
            lock (lockObj)
            {
                if (client != null && clientList.Contains(client))
                {
                    bool removed = clientList.Remove(client);
                    if (removed)
                    {
                         Console.WriteLine($"客户端管理器: 移除了客户端 (ID: {client.Id}, User: {client.userName ?? "N/A"})，剩余总数: {clientList.Count}");
                    }
                }
            }
        }

        /// <summary>
        /// 根据用户名查找客户端
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>找到的客户端，否则返回 null</returns>
        public Client GetClientByUserName(string userName)
        {
            lock (lockObj)
            {
                // 使用 FirstOrDefault 避免在未找到时抛出异常
                return clientList.FirstOrDefault(c => c.userName == userName);
            }
        }

        /// <summary>
        /// 根据 ID 查找客户端
        /// </summary>
        /// <param name="id">客户端 ID</param>
        /// <returns>找到的客户端，否则返回 null</returns>
        public Client GetClientByID(int id)
        {
            lock (lockObj)
            {
                // 使用 FirstOrDefault 避免在未找到时抛出异常
                return clientList.FirstOrDefault(c => c.Id == id);
            }
        }

        /// <summary>
        /// 获取所有客户端列表的副本（防止外部修改原始列表）
        /// </summary>
        /// <returns>客户端列表的副本</returns>
        public List<Client> GetAllClients()
        {
             lock (lockObj)
             {
                 return new List<Client>(clientList); // 返回副本
             }
        }

        /// <summary>
        /// 获取当前客户端数量
        /// </summary>
        /// <returns>客户端数量</returns>
        public int GetClientCount()
        {
            lock (lockObj)
            {
                return clientList.Count;
            }
        }
    }
}