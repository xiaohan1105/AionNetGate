using AionCommons.LogEngine;
using AionNetGate.Configs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AionNetGate.Services
{
    /// <summary>
    /// 账号服务 - 重构版
    /// 修复：SQL注入漏洞、连接池管理、异常处理
    /// </summary>
    class AccountService
    {
        internal static AccountService Instance = new AccountService();

        private static readonly Logger Logger = LoggerFactory.getLogger();

        // 参数化SQL语句 - 防止SQL注入
        private static readonly string MYSQL_CHECK_ACCOUNT = "SELECT * FROM account_data WHERE `name` = @name";
        private static readonly string MYSQL_ADD_EMAIL = "ALTER TABLE `account_data` ADD COLUMN `email` varchar(50) DEFAULT NULL AFTER `last_mac`";
        private static readonly string MYSQL_REG_ACCOUNT = "INSERT INTO account_data(`name`, `password`,`email`) VALUES (@name, @password, @email)";
        private static readonly string MYSQL_CHANGE_PASSWORD = "UPDATE account_data SET `password` = @newPsw WHERE `name` = @name and `password` = @oldPsw";
        private static readonly string MYSQL_FIND_PASSWORD = "UPDATE account_data SET `password` = @password WHERE `name` = @name and `email` = @email";

        // MSSQL 参数化SQL语句
        private static readonly string MSSQL_CHECK_ACCOUNT_NEW = "SELECT name FROM [dbo].[account_data] WHERE [name] = @name AND [password] = @password";
        private static readonly string MSSQL_CHECK_ACCOUNT_OLD = "SELECT account FROM [dbo].[user_auth] WHERE [account] = @name AND [password] = @password";
        private static readonly string MSSQL_REG_ACCOUNT_NEW = "INSERT INTO [dbo].[account_data]([name],[password],[email]) VALUES(@name, @password, @email)";
        private static readonly string MSSQL_CHECK_EXISTS_NEW = "SELECT COUNT(id) FROM [dbo].[account_data] WHERE [name] = @name";
        private static readonly string MSSQL_CHANGE_PASSWORD_NEW = "UPDATE [dbo].[account_data] SET [password] = @newPsw WHERE [name] = @name AND [password] = @oldPsw";
        private static readonly string MSSQL_CHANGE_PASSWORD_OLD = "UPDATE [dbo].[user_auth] SET [password] = @newPsw WHERE [account] = @name AND [password] = @oldPsw";
        private static readonly string MSSQL_FIND_PASSWORD_NEW = "UPDATE [dbo].[account_data] SET [password] = @password WHERE [name] = @name AND [email] = @email";
        private static readonly string MSSQL_FIND_PASSWORD_OLD = "UPDATE [dbo].[user_auth] SET [password] = @password WHERE [account] = @name AND [email] = @email";
        private static readonly string MSSQL_GET_ACCOUNT_INFO_NEW = "SELECT * FROM [dbo].[account_data] WHERE [name] = @name";
        private static readonly string MSSQL_GET_ACCOUNT_INFO_OLD = "SELECT * FROM [dbo].[user_auth] WHERE [account] = @name";

        // 账号名验证正则
        private static readonly Regex AccountNameRegex = new Regex(@"^[a-zA-Z0-9]{4,16}$");

        internal AccountService()
        {
            // 不再创建全局连接，每次操作使用新连接（连接池管理）
        }

        #region 公共接口

        /// <summary>
        /// 测试连接数据库是否成功
        /// </summary>
        internal bool ConnectionTest(string connectString, out string msg)
        {
            return Config.isMysql ? ConnectionTestMySQL(connectString, out msg) : ConnectionTestMSSQL(connectString, out msg);
        }

        /// <summary>
        /// 账号注册
        /// </summary>
        internal bool RegAccount(string name, string password, string email, out string message)
        {
            // 输入验证
            if (!ValidateAccountName(name, out message))
            {
                return false;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 4 || password.Length > 16)
            {
                message = "密码长度应为4-16位";
                return false;
            }

            return Config.isMysql ? RegAccountMySQL(name, password, email, out message) : RegAccountMSSQL(name, password, email, out message);
        }

        /// <summary>
        /// 登录器上验证账号和密码
        /// </summary>
        internal bool CheckAccountAndPassword(string name, string psw)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(psw))
            {
                return false;
            }

            return Config.isMysql ? CheckAccountPswMySQL(name, psw) : CheckAccountPswMSSQL(name, psw);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        internal bool ChangePassword(string name, string oldPsw, string newPsw, out string message)
        {
            if (string.IsNullOrEmpty(newPsw) || newPsw.Length < 4 || newPsw.Length > 16)
            {
                message = "新密码长度应为4-16位";
                return false;
            }

            return Config.isMysql ? ChangePasswordMySQL(name, oldPsw, newPsw, out message) : ChangePasswordMSSQL(name, oldPsw, newPsw, out message);
        }

        /// <summary>
        /// 找回密码（重置密码）
        /// </summary>
        internal bool FindPassword(string name, string email, out string psw, out string message)
        {
            return Config.isMysql ? FindPasswordMySQL(name, email, out psw, out message) : FindPasswordMSSQL(name, email, out psw, out message);
        }

        #endregion

        #region 输入验证

        /// <summary>
        /// 验证账号名格式
        /// </summary>
        private bool ValidateAccountName(string name, out string message)
        {
            message = "";

            if (string.IsNullOrEmpty(name))
            {
                message = "账号名不能为空";
                return false;
            }

            if (!AccountNameRegex.IsMatch(name))
            {
                message = "账号只能是4至16位的字母和数字";
                return false;
            }

            return true;
        }

        #endregion

        #region 数据库连接测试

        private bool ConnectionTestMSSQL(string connectString, out string msg)
        {
            msg = "数据库连接失败";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectString))
                {
                    conn.Open();
                    msg = "数据库连接成功,当前版本:" + conn.ServerVersion;
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = "数据库连接失败: " + ex.Message;
                Logger.error("MSSQL连接测试失败: {0}", ex.Message);
                return false;
            }
        }

        private bool ConnectionTestMySQL(string connectString, out string msg)
        {
            msg = "数据库连接失败";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectString))
                {
                    conn.Open();
                    msg = "数据库连接成功,当前版本:" + conn.ServerVersion;
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = "数据库连接失败: " + ex.Message;
                Logger.error("MySQL连接测试失败: {0}", ex.Message);
                return false;
            }
        }

        #endregion

        #region MySQL 账号操作

        private bool RegAccountMySQL(string name, string password, string email, out string message)
        {
            message = "账号[" + name + "]注册成功";

            try
            {
                using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    // 尝试添加email列（如果不存在）
                    try
                    {
                        using (MySqlCommand cmdAlter = new MySqlCommand(MYSQL_ADD_EMAIL, con))
                        {
                            cmdAlter.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException)
                    {
                        // 列已存在，忽略错误
                    }

                    // 执行注册
                    using (MySqlCommand cmd = new MySqlCommand(MYSQL_REG_ACCOUNT, con))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@password", EncodeBySHA1(password));
                        cmd.Parameters.AddWithValue("@email", email ?? "");

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            Logger.info("玩家成功注册了新账号[{0}]", name);
                            return true;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Message.Contains("Duplicate entry"))
                {
                    message = "账户[" + name + "]已存在";
                }
                else
                {
                    message = "注册失败：" + ex.Message;
                }
                Logger.error("MySQL注册账号失败: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                message = "注册失败：系统错误";
                Logger.error("MySQL注册账号异常: {0}", ex.Message);
            }

            return false;
        }

        private bool CheckAccountPswMySQL(string name, string psw)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(MYSQL_CHECK_ACCOUNT, con))
                    {
                        cmd.Parameters.AddWithValue("@name", name);

                        using (MySqlDataReader rs = cmd.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                string storedPassword = rs.GetString("password");
                                return storedPassword.Equals(EncodeBySHA1(psw));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("MySQL验证账号[{0}]密码失败: {1}", name, ex.Message);
            }

            return false;
        }

        private bool ChangePasswordMySQL(string name, string oldPsw, string newPsw, out string message)
        {
            message = "密码修改失败，可能原密码不对！";

            try
            {
                using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(MYSQL_CHANGE_PASSWORD, con))
                    {
                        cmd.Parameters.AddWithValue("@newPsw", EncodeBySHA1(newPsw));
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@oldPsw", EncodeBySHA1(oldPsw));

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            message = "账号[" + name + "]的密码已成功修改";
                            Logger.info("玩家成功修改了账号[{0}]的密码", name);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("MySQL修改密码失败: {0}", ex.Message);
            }

            return false;
        }

        private bool FindPasswordMySQL(string name, string email, out string psw, out string message)
        {
            psw = null;
            message = "密码找回失败，账号或者邮箱不存在！";

            try
            {
                Random rnd = new Random();
                string newPsw = rnd.Next(100000, 999999).ToString();

                using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand(MYSQL_FIND_PASSWORD, con))
                    {
                        cmd.Parameters.AddWithValue("@password", EncodeBySHA1(newPsw));
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@email", email);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            psw = newPsw;
                            message = "账号[" + name + "]的密码已发到指定邮箱，请稍后从邮箱中查看新密码。";
                            Logger.info("玩家申请找回了账号[{0}]的密码", name);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("MySQL找回密码失败: {0}", ex.Message);
            }

            return false;
        }

        #endregion

        #region MSSQL 账号操作

        private bool RegAccountMSSQL(string name, string password, string email, out string message)
        {
            message = "账号[" + name + "]注册成功";
            string encodedPsw = EncodePasswordHash(password);

            try
            {
                using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    if (Config.newaccountdatabase)
                    {
                        // 新数据库结构
                        // 先检查账号是否存在
                        using (SqlCommand cmdCheck = new SqlCommand(MSSQL_CHECK_EXISTS_NEW, con))
                        {
                            cmdCheck.Parameters.AddWithValue("@name", name);
                            int exists = Convert.ToInt32(cmdCheck.ExecuteScalar());
                            if (exists > 0)
                            {
                                message = "账号[" + name + "]已经被人注册了";
                                return false;
                            }
                        }

                        // 执行注册
                        using (SqlCommand cmd = new SqlCommand(MSSQL_REG_ACCOUNT_NEW, con))
                        {
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@password", encodedPsw.Substring(2));
                            cmd.Parameters.AddWithValue("@email", email ?? "");

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                Logger.info("玩家成功注册了新账号[{0}]", name);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // 传统数据库结构 - 使用参数化查询
                        string sqlAccount = "INSERT INTO [dbo].[user_account]([account]) VALUES (@name)";
                        using (SqlCommand cmdAccount = new SqlCommand(sqlAccount, con))
                        {
                            cmdAccount.Parameters.AddWithValue("@name", name);
                            if (cmdAccount.ExecuteNonQuery() > 0)
                            {
                                string sqlAuth = "INSERT INTO [dbo].[user_auth] ([account], [password], [quiz1], [quiz2], [answer1], [answer2], [new_pwd_flag], [lastat], [email]) VALUES(@name, @password, '1', '2', 0, 0, 0, NULL, @email)";
                                using (SqlCommand cmdAuth = new SqlCommand(sqlAuth, con))
                                {
                                    cmdAuth.Parameters.AddWithValue("@name", name);
                                    cmdAuth.Parameters.AddWithValue("@password", encodedPsw);
                                    cmdAuth.Parameters.AddWithValue("@email", email ?? "");

                                    int result = cmdAuth.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        Logger.info("玩家成功注册了新账号[{0}]", name);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("重复键") || ex.Message.Contains("Duplicate"))
                {
                    message = "账户[" + name + "]已存在";
                }
                else
                {
                    message = "注册失败：" + ex.Message;
                }
                Logger.error("MSSQL注册账号失败: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                message = "注册失败：系统错误";
                Logger.error("MSSQL注册账号异常: {0}", ex.Message);
            }

            return false;
        }

        private bool CheckAccountPswMSSQL(string name, string psw)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    string sql = Config.newaccountdatabase ? MSSQL_CHECK_ACCOUNT_NEW : MSSQL_CHECK_ACCOUNT_OLD;
                    string encodedPsw = EncodePasswordHash(psw);

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@name", name);

                        if (Config.newaccountdatabase)
                        {
                            cmd.Parameters.AddWithValue("@password", encodedPsw.Substring(2));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@password", encodedPsw);
                        }

                        object result = cmd.ExecuteScalar();
                        return result != null && result.ToString() == name;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("MSSQL验证账号[{0}]密码失败: {1}", name, ex.Message);
            }

            return false;
        }

        private bool ChangePasswordMSSQL(string name, string oldPsw, string newPsw, out string message)
        {
            message = "密码修改失败，可能原密码不对！";

            try
            {
                using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    string sql = Config.newaccountdatabase ? MSSQL_CHANGE_PASSWORD_NEW : MSSQL_CHANGE_PASSWORD_OLD;
                    string encodedOldPsw = EncodePasswordHash(oldPsw);
                    string encodedNewPsw = EncodePasswordHash(newPsw);

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@name", name);

                        if (Config.newaccountdatabase)
                        {
                            cmd.Parameters.AddWithValue("@oldPsw", encodedOldPsw.Substring(2));
                            cmd.Parameters.AddWithValue("@newPsw", encodedNewPsw.Substring(2));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@oldPsw", encodedOldPsw);
                            cmd.Parameters.AddWithValue("@newPsw", encodedNewPsw);
                        }

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            message = "账号[" + name + "]的密码已成功修改";
                            Logger.info("玩家成功修改了账号[{0}]的密码", name);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("MSSQL修改密码失败: {0}", ex.Message);
            }

            return false;
        }

        private bool FindPasswordMSSQL(string name, string email, out string psw, out string message)
        {
            psw = null;
            message = "密码找回失败，账号或者邮箱不存在！";

            try
            {
                Random rnd = new Random();
                string newPsw = rnd.Next(100000, 999999).ToString();
                string encodedPsw = EncodePasswordHash(newPsw);

                using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls)))
                {
                    con.Open();

                    string sql = Config.newaccountdatabase ? MSSQL_FIND_PASSWORD_NEW : MSSQL_FIND_PASSWORD_OLD;

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@email", email);

                        if (Config.newaccountdatabase)
                        {
                            cmd.Parameters.AddWithValue("@password", encodedPsw.Substring(2));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@password", encodedPsw);
                        }

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            psw = newPsw;
                            message = "账号[" + name + "]的密码已发到指定邮箱，请稍后从邮箱中查看新密码。";
                            Logger.info("玩家申请找回了账号[{0}]的密码", name);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("MSSQL找回密码失败: {0}", ex.Message);
            }

            return false;
        }

        #endregion

        #region 密码加密

        /// <summary>
        /// SHA1方式加密 (MySQL)
        /// </summary>
        private string EncodeBySHA1(string password)
        {
            using (System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                byte[] tmpByte = sha1.ComputeHash(Encoding.UTF8.GetBytes(password.ToCharArray()));
                return Convert.ToBase64String(tmpByte);
            }
        }

        /// <summary>
        /// 真端的密码加密 (MSSQL)
        /// </summary>
        public static string EncodePasswordHash(string password)
        {
            return "0x" + BitConverter.ToString(GetAccountPasswordHash(password), 0).Replace("-", string.Empty).ToUpper();
        }

        private static byte[] GetAccountPasswordHash(string input)
        {
            byte[] buffer = new byte[0x11];
            byte[] src = new byte[0x11];
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            for (int i = 0; i < input.Length; i++)
            {
                buffer[i + 1] = bytes[i];
                src[i + 1] = buffer[i + 1];
            }
            long num = ((buffer[1] + (buffer[2] * 0x100L)) + (buffer[3] * 0x10000L)) + (buffer[4] * 0x1000000);
            long num2 = (num * 0x3407fL) + 0x269735L;
            num2 -= (num2 / 0x100000000L) * 0x100000000L;
            num = ((buffer[5] + (buffer[6] * 0x100)) + (buffer[7] * 0x10000L)) + (buffer[8] * 0x1000000);
            long num3 = (num * 0x340ff) + 0x269741;
            num3 -= (num3 / 0x100000000) * 0x100000000;
            num = ((buffer[9] + (buffer[10] * 0x100L)) + (buffer[11] * 0x10000L)) + (buffer[12] * 0x1000000);
            long num4 = (num * 0x340d3) + 0x269935;
            num4 -= (num4 / 0x100000000) * 0x100000000L;
            num = ((buffer[13] + (buffer[14] * 0x100)) + (buffer[15] * 0x10000L)) + (buffer[0x10] * 0x1000000);
            long num5 = (num * 0x3433d) + 0x269acdL;
            num5 -= (num5 / 0x100000000) * 0x100000000;
            buffer[4] = (byte)(num2 / 0x1000000L);
            buffer[3] = (byte)((num2 - (buffer[4] * 0x1000000)) / 0x10000L);
            buffer[2] = (byte)((num2 - (buffer[4] * 0x1000000) - (buffer[3] * 0x10000)) / 0x100L);
            buffer[1] = (byte)(num2 - (buffer[4] * 0x1000000) - (buffer[3] * 0x10000) - (buffer[2] * 0x100));
            buffer[8] = (byte)(num3 / 0x1000000L);
            buffer[7] = (byte)((num3 - (buffer[8] * 0x1000000L)) / 0x10000L);
            buffer[6] = (byte)((num3 - (buffer[8] * 0x1000000L) - (buffer[7] * 0x10000)) / 0x100L);
            buffer[5] = (byte)(num3 - (buffer[8] * 0x1000000L) - (buffer[7] * 0x10000) - (buffer[6] * 0x100));
            buffer[12] = (byte)(num4 / 0x1000000L);
            buffer[11] = (byte)((num4 - (buffer[12] * 0x1000000L)) / (0x10000L));
            buffer[10] = (byte)((num4 - (buffer[12] * 0x1000000L) - (buffer[11] * 0x10000)) / 0x100);
            buffer[9] = (byte)(num4 - (buffer[12] * 0x1000000L) - (buffer[11] * 0x10000) - (buffer[10] * 0x100));
            buffer[0x10] = (byte)(num5 / 0x1000000L);
            buffer[15] = (byte)((num5 - (buffer[0x10] * 0x1000000L)) / 0x10000L);
            buffer[14] = (byte)((num5 - (buffer[0x10] * 0x1000000L) - (buffer[15] * 0x10000)) / 0x100);
            buffer[13] = (byte)(num5 - (buffer[0x10] * 0x1000000L) - (buffer[15] * 0x10000) - (buffer[14] * 0x100));
            src[1] = (byte)(src[1] ^ buffer[1]);
            int index = 1;
            while (index < 0x10)
            {
                index++;
                src[index] = (byte)((src[index] ^ src[index - 1]) ^ buffer[index]);
            }
            index = 0;
            while (index < 0x10)
            {
                index++;
                if (src[index] == 0)
                {
                    src[index] = 0x66;
                }
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            return dst;
        }

        #endregion

        #region 通用查询方法

        /// <summary>
        /// 返回DataSet - 使用参数化查询
        /// </summary>
        public DataSet ExecuteSelectCommand(string cmdText, Dictionary<string, object> parameters = null)
        {
            DataSet ds = new DataSet();

            try
            {
                if (Config.isMysql)
                {
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_gs)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                        {
                            if (parameters != null)
                            {
                                foreach (var param in parameters)
                                {
                                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                                }
                            }

                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                            {
                                da.Fill(ds);
                            }
                        }
                    }
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_gs)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(cmdText, con))
                        {
                            if (parameters != null)
                            {
                                foreach (var param in parameters)
                                {
                                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                                }
                            }

                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(ds);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("执行查询失败: {0}", ex.Message);
            }

            return ds;
        }

        /// <summary>
        /// 执行SQL命令 - 使用参数化查询
        /// </summary>
        public int ExecuteNonQuery(string cmdText, Dictionary<string, object> parameters = null)
        {
            int result = 0;

            try
            {
                if (Config.isMysql)
                {
                    using (MySqlConnection con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_gs)))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                        {
                            if (parameters != null)
                            {
                                foreach (var param in parameters)
                                {
                                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                                }
                            }
                            result = cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_gs)))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(cmdText, con))
                        {
                            if (parameters != null)
                            {
                                foreach (var param in parameters)
                                {
                                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                                }
                            }
                            result = cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.error("执行命令失败: {0}", ex.Message);
            }

            return result;
        }

        /// <summary>
        /// 获取在线账号统计
        /// </summary>
        public int GetOnlineAccountCount()
        {
            string sql;

            if (Config.isMysql)
            {
                sql = "SELECT COUNT(*) FROM account_data WHERE last_login_time > last_logout_time";
            }
            else if (Config.newaccountdatabase)
            {
                sql = "SELECT COUNT(*) FROM [dbo].[account_data] WHERE last_login_time > last_logout_time";
            }
            else
            {
                sql = "SELECT COUNT(*) FROM [dbo].[user_data] WHERE last_login_time > last_logout_time";
            }

            try
            {
                DataSet ds = ExecuteSelectCommand(sql);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                Logger.error("获取在线账号统计失败：{0}", ex.Message);
            }

            return 0;
        }

        /// <summary>
        /// 获取账号信息 - 使用参数化查询
        /// </summary>
        public DataSet GetAccountInfo(string accountName)
        {
            string sql;

            if (Config.isMysql)
            {
                sql = "SELECT * FROM account_data WHERE `name` = @name";
            }
            else if (Config.newaccountdatabase)
            {
                sql = MSSQL_GET_ACCOUNT_INFO_NEW;
            }
            else
            {
                sql = MSSQL_GET_ACCOUNT_INFO_OLD;
            }

            var parameters = new Dictionary<string, object>
            {
                { "@name", accountName }
            };

            return ExecuteSelectCommand(sql, parameters);
        }

        #endregion

        #region 兼容旧接口

        /// <summary>
        /// 兼容旧的 ExecuteSelectCmmond 方法名
        /// </summary>
        [Obsolete("请使用 ExecuteSelectCommand 方法")]
        public DataSet ExecuteSelectCmmond(string cmdText)
        {
            return ExecuteSelectCommand(cmdText);
        }

        #endregion
    }
}
