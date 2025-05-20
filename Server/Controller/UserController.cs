using SoulKnightProtocol;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Internal;

namespace KnightServer
{
    public class UserController : BaseController
    {
        public UserController(ControllerManager manager) : base(manager)
        {
            requestCode = RequestCode.User;
        }
        public MainPack Register(Client client, MainPack pack)
        {
            string username = pack.LoginPack.UserName;
            string password = pack.LoginPack.Password;

            Console.WriteLine($"[注册流程] 收到注册请求，用户名: {username}");

            if (username.Length < 5 || username.Length > 25)
            {
                Console.WriteLine($"[注册失败] 用户名长度不合法: {username.Length}");
                pack.ReturnCode = ReturnCode.Fail;
                pack.Str = "用户名长度必须在5-10个字符之间";
                return pack;
            }

            // 2. 检查密码是否同时包含数字和英文字母
            if (!Regex.IsMatch(password, @"\d") || !Regex.IsMatch(password, @"[a-zA-Z]"))
            {
                Console.WriteLine($"[注册失败] 密码不符合要求: {password}");
                pack.ReturnCode = ReturnCode.Fail;
                pack.Str = "密码必须同时包含数字和英文字母";
                Console.WriteLine("密码必须同时包含数字和英文字母");
                return pack;
            }

            // 检查用户名是否已存在
            if (client.m_UserDao.IsUserExist(username))
            {
                Console.WriteLine($"[注册失败] 用户名已存在: {username}");
                pack.ReturnCode = ReturnCode.Fail;
                pack.Str = "用户名已存在";
                return pack;
            }

            // 3. 对密码进行加密
            string encryptedPassword = EncryptPassword(password);
            Console.WriteLine($"[注册流程] 加密后密码: {encryptedPassword}");

            // 创建新的包含加密密码的登录包
            LoginPack loginPack = new LoginPack
            {
                UserName = username,
                Password = encryptedPassword
            };

            // 替换原始登录包
            MainPack newPack = new MainPack
            {
                RequestCode = pack.RequestCode,
                ActionCode = pack.ActionCode,
                LoginPack = loginPack
            };

            bool dbResult = client.m_UserDao.Register(newPack);
            Console.WriteLine($"[注册流程] 数据库写入结果: {dbResult}");

            if (dbResult)
            {
                Console.WriteLine($"[注册成功] 用户名: {username}");
                pack.ReturnCode = ReturnCode.Success;
            }
            else
            {
                Console.WriteLine($"[注册失败] 数据库写入失败，用户名: {username}");
                pack.ReturnCode = ReturnCode.Fail;
                pack.Str = "注册失败";
            }
            return pack;
        }

        public MainPack Login(Client client, MainPack pack)
        {
            // 对输入的密码进行加密，以便与数据库中的加密密码比较
            string encryptedPassword = EncryptPassword(pack.LoginPack.Password);
            
            // 创建新的包含加密密码的登录包
            LoginPack loginPack = new LoginPack
            {
                UserName = pack.LoginPack.UserName,
                Password = encryptedPassword
            };
            Client client1 = ClientManager.Instance.GetClientByUserName(pack.LoginPack.UserName);
            if (client1 != null)
            {
                Console.WriteLine($"[登录流程] 用户名: {pack.LoginPack.UserName}，登录结果: 已登录");
                pack.ReturnCode = ReturnCode.Fail;
                pack.Str = "已登录";
                return pack;
            }
            // 替换原始登录包
            MainPack newPack = new MainPack
            {
                RequestCode = pack.RequestCode,
                ActionCode = pack.ActionCode,
                LoginPack = loginPack
            };

            if (client.m_UserDao.Login(newPack))
            {
                pack.ReturnCode = ReturnCode.Success;
                client.userName = pack.LoginPack.UserName;
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }
            Console.WriteLine($"[登录流程] 用户名: {pack.LoginPack.UserName}，登录结果: {pack.ReturnCode}");
            return pack;
        }

        // 密码加密方法
        private string EncryptPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // 将密码转换为字节数组
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                
                // 计算哈希值
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                
                // 将哈希值转换为十六进制字符串
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                
                return builder.ToString();
            }
        }
    }
}
