using MySql.Data.MySqlClient;
using SoulKnightProtocol;
using System.Data;

public class UserDao
{
    private MySqlConnection conn;
    private string s = "database=soulknight; data source=localhost;user=root;password=123456;pooling=false;charset=utf8;port=3306";
    private static bool isTableChecked = false; // 静态变量，所有实例共享

    public UserDao()
    {
        ConnectDatabase();
        // 只在第一次创建UserDao时检查和创建表
        if (!isTableChecked)
        {
            CheckAndCreateTables();
            isTableChecked = true;
        }
    }
    
    private void ConnectDatabase()
    {
        try
        {
            conn = new MySqlConnection(s);
            conn.Open();
        }
        catch (Exception)
        {
            Console.WriteLine("连接数据库失败");
        }
    }
    
    // 添加检查和创建表的方法
    private void CheckAndCreateTables()
    {
        try
        {
            // 先尝试删除已存在的user表
            string dropTableSql = "DROP TABLE IF EXISTS user";
            MySqlCommand dropCmd = new MySqlCommand(dropTableSql, conn);
            dropCmd.ExecuteNonQuery();
            Console.WriteLine("已删除旧的user表");
            
            // 创建新的user表
            string createTableSql = @"CREATE TABLE user (
                id INT AUTO_INCREMENT PRIMARY KEY,
                UserName VARCHAR(50) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL
            )";
            
            MySqlCommand createCmd = new MySqlCommand(createTableSql, conn);
            createCmd.ExecuteNonQuery();
            Console.WriteLine("user表创建成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"检查或创建表失败: {ex.Message}");
        }
    }
    
    public bool Register(MainPack pack)
    {
        string userName = pack.LoginPack.UserName;
        string password = pack.LoginPack.Password;
        string sql = "Insert into user(UserName,Password) values(@userName,@password)";
        MySqlCommand cmd = new MySqlCommand(sql, conn);
        // 修改参数类型，使用 MySqlDbType.VarChar 替代 SqlDbType.NVarChar
        cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = userName;
        cmd.Parameters.Add("@password", MySqlDbType.VarChar).Value = password;
        try
        {
            cmd.ExecuteNonQuery();
            Console.WriteLine("数据库成功写入了一条数据");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"数据库插入数据失败: {ex.Message}");
            Console.WriteLine($"异常详情: {ex.StackTrace}");
            return false;
        }
        return true;
    }
    public bool Login(MainPack pack)
    {
        bool isFind = false;
        string sql = "Select * from user where UserName=@username and Password=@password";
        MySqlCommand cmd = new MySqlCommand(sql, conn);
        // 修改参数类型，使用 MySqlDbType.VarChar 替代 SqlDbType.NVarChar
        cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = pack.LoginPack.UserName;
        cmd.Parameters.Add("@password", MySqlDbType.VarChar).Value = pack.LoginPack.Password;
        try
        {
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                isFind = true;
            }
            reader.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"数据库查询数据失败: {ex.Message}");
            Console.WriteLine($"异常详情: {ex.StackTrace}");
        }

        return isFind;
    }
    public void CloseConn()
    {
        conn.Close();
    }
    
    // 修复 IsUserExist 方法
    public bool IsUserExist(string userName)
    {
        try
        {
            MySqlCommand cmd = new MySqlCommand("select * from user where UserName = @username", conn);
            cmd.Parameters.AddWithValue("@username", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool exist = reader.HasRows;
            reader.Close();
            return exist;
        }
        catch (Exception e)
        {
            Console.WriteLine("在检查用户是否存在时出现异常: " + e.Message);
            return false;
        }
    }
}