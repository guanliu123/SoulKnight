using MySql.Data.MySqlClient;
using SoulKnightProtocol;
using System.Data;

public class UserDao
{
    private MySqlConnection conn;
    private string s = "database=soulknight; data source=localhost;user=root;password=20121221;pooling=false;charset=utf8;port=3306";
    public UserDao()
    {
        ConnectDatabase();
        // 连接成功后检查并创建表
        CheckAndCreateTables();
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
            // 检查user表是否存在
            string checkTableSql = "SHOW TABLES LIKE 'user'";
            MySqlCommand checkCmd = new MySqlCommand(checkTableSql, conn);
            object result = checkCmd.ExecuteScalar();
            
            // 如果表不存在，则创建
            if (result == null)
            {
                string createTableSql = @"CREATE TABLE user (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    UserName VARCHAR(50) NOT NULL UNIQUE,
                    Password VARCHAR(50) NOT NULL
                )";
                
                MySqlCommand createCmd = new MySqlCommand(createTableSql, conn);
                createCmd.ExecuteNonQuery();
                Console.WriteLine("user表创建成功");
            }
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
}