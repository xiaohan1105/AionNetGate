using AionCommons.LogEngine;
using AionNetGate.Configs;
using AionNetGate.Netwok;
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
    class AccountService
    {
        internal static AccountService Instance = new AccountService();

        private static readonly Logger Logger = LoggerFactory.getLogger(); // Loggerger instance.

        private static readonly string CHECK_ACCOUNT   = "SELECT * FROM account_data WHERE `name` = @name";
        private static readonly string ADD_EMAIL       = "ALTER TABLE `account_data`  ADD COLUMN `email` varchar(50) DEFAULT NULL AFTER `last_mac`";
        private static readonly string REG_ACCOUNT     = "INSERT INTO account_data(`name`, `password`,`email`) VALUES (@name, @password, @email)";
        private static readonly string CHANGE_PASSWORD = "UPDATE account_data SET `password` = @newPsw WHERE `name` = @name and `password` = @oldPsw";
        private static readonly string FIND_PASSWORD   = "UPDATE account_data SET `password` = @password WHERE `name` = @name and `email` = @email";

        private MySqlConnection con;
        private SqlConnection mscon;
        private SqlConnection gameCon;

        internal AccountService()
        {
            con = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_ls));
            mscon = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_ls));
            gameCon = new SqlConnection(Config.GetMSSQLConnectionString(Config.mysql_db_gs));
        }
        /// <summary>
        /// 测试连接数据库是否成功
        /// </summary>
        /// <returns></returns>
        internal bool ConnectionTest(string connectString, out string msg)
        {
            return Config.isMysql ? connectionTest(connectString, out msg) : connectionTestMSSQL(connectString, out msg);
        }

        /// <summary>
        /// 账号注册
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal bool RegAccount(string name, string password, string email, out string message)
        {
            return Config.isMysql ? RegAccountMySQL(name, password, email, out message) : RegAccountMSSQL(name, password, email, out message);
        }

        /// <summary>
        /// 登录器上验证账号和密码
        /// </summary>
        /// <param name="name"></param>
        /// <param name="psw"></param>
        /// <returns></returns>
        internal bool CheckAccountAndPassword(string name, string psw)
        {
            return Config.isMysql ? CheckAccountPswMysql(name, psw) : CheckAccountPswMssql(name, psw);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="con"></param>
        /// <param name="name"></param>
        /// <param name="oldPsw"></param>
        /// <param name="newPsw"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal bool ChangePassword(string name, string oldPsw, string newPsw, out string message)
        {
            return Config.isMysql ? ChangePasswordMysql(name, oldPsw, newPsw, out message) : ChangePasswordMssql(name, oldPsw, newPsw, out message);
        }

        /// <summary>
        /// 找回密码（重置密码）
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="email">邮箱</param>
        /// <param name="psw">新密码</param>
        /// <param name="message">信息</param>
        /// <returns></returns>
        internal bool FindPassword(string name, string email, out string psw, out string message)
        {
            return Config.isMysql ? FindPasswordMysql(name, email, out psw, out message) : FindPasswordMssql(name, email, out psw, out message);
        }



        #region 测试数据库连接是否正常
        private static bool connectionTestMSSQL(string connectString, out string msg)
        {
            msg = "数据库连接失败:";
            SqlConnection conn = new SqlConnection(connectString);
            bool isCorrect = false;
            try
            {
                conn.Open();  //打开数据库  
                msg = "数据库连接成功,当前版本:" + conn.ServerVersion;
                isCorrect = true;
            }
            catch
            {

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();    //关闭数据库  
                }
            }
            return isCorrect;
        }
        private static bool connectionTest(string connectString, out string msg)
        {
            msg = "数据库连接失败:";
            bool isCorrect = false;
            MySqlConnection con = new MySqlConnection();
            con.ConnectionString = connectString;
            try
            {
                con.Open();
                msg = "数据库连接成功,当前版本:" + con.ServerVersion;
                isCorrect = true;
            }
            catch (Exception ex)
            {
                msg += ex.Message;
            }
            finally
            {
                con.Close();
            }
            return isCorrect;
        }
        #endregion

        #region 账号注册
        private bool RegAccountMySQL(string name, string password, string email, out string message) 
        { 
            MySqlCommand cmd = new MySqlCommand(ADD_EMAIL, con);
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
            finally
            {
                cmd.Dispose();
            }

            message = "账号[" + name + "]注册成功";
            int result = 0;
            cmd = new MySqlCommand(REG_ACCOUNT, con);
            MySqlParameterCollection st = cmd.Parameters;
            try
            {
                st.AddWithValue("@name", name);
                st.AddWithValue("@password", EncodeBySHA1(password));
                st.AddWithValue("@email", email);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                message = "注册失败：" + (e.Message.StartsWith("Duplicate entry") ? "账户[" + name + "]已存在" : e.Message);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
            bool success = result > 0;
            if (success)
            {
                Logger.info("玩家成功注册了新账号[{0}]", name);
            }

            return success;
        }

        /// <summary>
        /// MSSQL账号注册
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool RegAccountMSSQL(string name, string password, string email, out string message)
        {
            Regex PwdRegex = new Regex(@"^[\x20-\xFF]{4,16}$");
            if (!PwdRegex.IsMatch(name))
            {
                message = "账号只能是4至16位的字母和数字";
                return false;
            }
            string psw = EncodePasswordHash(password);

            message = "账号[" + name + "]注册成功";

            string sql = string.Format("if not exists(select * from syscolumns where id=object_id('dbo.user_auth') and name='email') " +
                "BEGIN " +
                "   ALTER TABLE[dbo].[user_auth] ADD[email] varchar(50) DEFAULT NULL; " +
                "END " +
                "   INSERT INTO [dbo].[user_account]([account]) VALUES ('{0}');", name);

            SqlCommand cmd = new SqlCommand(sql, mscon);
            int result = 0;
            try
            {
                mscon.Open();


                //新的4.6数据库结构，镜子的那份
                if (Config.newaccountdatabase)
                {
                    sql = string.Format("SELECT count(id) FROM [dbo].[account_data] WHERE [dbo].[account_data].name = '{0}';", name);
                    cmd = new SqlCommand(sql, mscon);
                    result = Convert.ToInt32(cmd.ExecuteScalar());
                    if (result == 0)
                    {
                        sql = string.Format("INSERT INTO [dbo].[account_data]([name],[password],[email])VALUES('{0}', '{1}', '{2}'); ", name, psw.Substring(2), email);

                        cmd = new SqlCommand(sql, mscon);
                        result = cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        result = -1;
                        message = "账号[" + name + "]已经被人注册掉了";
                    }
                }
                else
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        sql = string.Format("INSERT INTO [dbo].[user_auth] ([account], [password], [quiz1], [quiz2], [answer1], [answer2], [new_pwd_flag], [lastat],[email])VALUES('{0}',{1}, '1', '2', 0, 0, 0, NULL,'{2}'); ", name, psw, email);
                        cmd = new SqlCommand(sql, mscon);
                        result = cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                message = "注册失败：" + (e.Message.Contains("重复键") ? "账户[" + name + "]已存在" : e.Message);
            }
            finally
            {
                cmd.Dispose();
                mscon.Close();
            }

            bool success = result > 0;
            if (success)
            {
                Logger.info("玩家成功注册了新账号[{0}]", name);
            }
            return success;
        }



        #endregion

        #region 验证账号密码
        private bool CheckAccountPswMysql(string name, string psw) 
        { 
            bool isCorrect = false;
            MySqlCommand cmd = new MySqlCommand(CHECK_ACCOUNT, con);
            cmd.Parameters.Add("@name", MySqlDbType.String).Value = name;
            try
            {
                con.Open();
                MySqlDataReader rs = cmd.ExecuteReader();
                if (rs.Read())
                {
                    isCorrect = rs.GetString("password").Equals(EncodeBySHA1(psw));
                }
            }
            catch (Exception e)
            {
                Logger.error("验证账号[{0}]和密码[{1}]遇到问题: ", name, psw, e.Message);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
            return isCorrect;
        }
        private bool CheckAccountPswMssql(string name, string psw)
        {
            bool isCorrect = false;
            string sql;

            // 支持新的4.6数据库结构和传统结构
            if (Config.newaccountdatabase)
            {
                sql = string.Format("SELECT name FROM [dbo].[account_data] WHERE [name] = '{0}' and [password] = '{1}'", name, EncodePasswordHash(psw).Substring(2));
            }
            else
            {
                sql = string.Format("SELECT account FROM [dbo].[user_auth] WHERE [account] = '{0}' and password={1}", name, EncodePasswordHash(psw));
            }

            SqlCommand cmd = new SqlCommand(sql, mscon);
            try
            {
                mscon.Open();
                string result = cmd.ExecuteScalar() as string;
                isCorrect = result == name;
            }
            catch (SqlException e)
            {
                Logger.error("验证账号[{0}]和密码[{1}]遇到问题:{2} ", name, psw, e.Message);
            }
            finally
            {
                cmd.Dispose();
                mscon.Close();
            }
            return isCorrect;
        }
        #endregion

        #region 修改密码
        private bool ChangePasswordMysql(string name, string oldPsw, string newPsw ,out string message)
        {
            message = "密码修改失败，可能原密码不对！";
            int result = 0;
            MySqlCommand cmd = new MySqlCommand(CHANGE_PASSWORD, con);
            MySqlParameterCollection st = cmd.Parameters;
            try
            {
                con.Open();
                st.AddWithValue("@newPsw", EncodeBySHA1(newPsw));
                st.AddWithValue("@name", name);
                st.AddWithValue("@oldPsw", EncodeBySHA1(oldPsw));
                result = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Logger.error("玩家修改密码遇到了问题",e.Message);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
            bool success = result > 0;
            if (success)
            {
                message = "账号[" + name + "]的密码已成功修改为[" + newPsw + "]";
                Logger.info("玩家成功修改了账号[{0}]的密码", name);
            }
            return success;
        }

        private bool ChangePasswordMssql(string name, string oldPsw, string newPsw, out string message)
        {
            message = "密码修改失败，可能原密码不对！";
            int result = -1;
            string sql;

            // 支持新的4.6数据库结构和传统结构
            if (Config.newaccountdatabase)
            {
                sql = string.Format("UPDATE [dbo].[account_data] SET [password] = '{0}' WHERE [name] = '{1}' and [password] = '{2}'",
                    EncodePasswordHash(newPsw).Substring(2), name, EncodePasswordHash(oldPsw).Substring(2));
            }
            else
            {
                sql = string.Format("UPDATE [dbo].[user_auth] SET [password] = {0} WHERE [account] = '{1}' and [password] = {2}",
                    EncodePasswordHash(newPsw), name, EncodePasswordHash(oldPsw));
            }

            SqlCommand cmd = new SqlCommand(sql, mscon);
            try
            {
                mscon.Open();
                result = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Logger.error("玩家修改密码遇到了问题", e.Message);
            }
            finally
            {
                cmd.Dispose();
                mscon.Close();
            }
            bool success = result > 0;
            if (success)
            {
                message = "账号[" + name + "]的密码已成功修改为[" + newPsw + "]";
                Logger.info("玩家成功修改了账号[{0}]的密码", name);
            }
            return success;
        }
        #endregion

        #region 找回密码
        private bool FindPasswordMysql(string name,string email,out string psw,out string message)
        {
            psw = null;
            message = "密码找回失败，账号或者邮箱不存在！";

            Random rnd = new Random();
            int newPsw = rnd.Next(10000, 1000000);
            int result = 0;
            MySqlCommand cmd = new MySqlCommand(FIND_PASSWORD, con);
            MySqlParameterCollection st = cmd.Parameters;
            try
            {
                con.Open();
                st.AddWithValue("@password", EncodeBySHA1(newPsw.ToString()));
                st.AddWithValue("@name", name);
                st.AddWithValue("@email", email);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Logger.error("玩家修改密码遇到了问题", e.Message);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
            bool success = result > 0;
            if (success)
            {
                psw = newPsw.ToString();
                message = "账号[" + name + "]的密码已发到指定邮箱，请稍后从邮箱中查看新密码。";
                Logger.info("玩家申请找回了账号[{0}]的密码", name);
            }
            return success;
        }
        /// <summary>
        /// 找回密码（重置密码）
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="email">邮箱</param>
        /// <param name="psw">新密码</param>
        /// <param name="message">信息</param>
        /// <returns></returns>
        internal bool FindPasswordMssql(string name, string email, out string psw, out string message)
        {
            psw = null;
            message = "密码找回失败，账号或者邮箱不存在！";

            Random rnd = new Random();
            string newPsw = rnd.Next(10000, 1000000).ToString();
            int result = -1;
            string sql;

            // 支持新的4.6数据库结构和传统结构
            if (Config.newaccountdatabase)
            {
                sql = string.Format("UPDATE [dbo].[account_data] SET [password] = '{0}' WHERE [name] = '{1}' and [email] = '{2}';",
                    EncodePasswordHash(newPsw).Substring(2), name, email);
            }
            else
            {
                sql = string.Format("UPDATE [dbo].[user_auth] SET [password] = {0} WHERE [account] = '{1}' and [email] = '{2}';",
                    EncodePasswordHash(newPsw), name, email);
            }

            SqlCommand cmd = new SqlCommand(sql, mscon);
            try
            {
                mscon.Open();
                result = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Logger.error("玩家修改密码遇到了问题", e.Message);
            }
            finally
            {
                cmd.Dispose();
                mscon.Close();
            }
            bool success = result > 0;
            if (success)
            {
                psw = newPsw;
                message = "账号[" + name + "]的密码已发到指定邮箱，请稍后从邮箱中查看新密码。";
                Logger.info("玩家申请找回了账号[{0}]的密码", name);
            }
            return success;
        }
        #endregion

        #region 密码加密方式
        /// <summary>
        /// SHA1方式加密
        /// </summary>
        /// <param name="password">密码字符串</param>
        /// <returns>返回SH1加密并转换成BASE64格式</returns>
        private string EncodeBySHA1(string password)
        {
            System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] tmpByte = sha1.ComputeHash(Encoding.UTF8.GetBytes(password.ToCharArray()));
            sha1.Clear();
            return Convert.ToBase64String(tmpByte);
        }
        /// <summary>
        /// 真端的密码加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
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





        /// <summary>
        /// 返回DataSet - 支持MySQL和MSSQL
        /// </summary>
        /// <param name="cmdText">SQL语句</param>
        /// <returns></returns>
        public DataSet ExecuteSelectCmmond(string cmdText)
        {
            DataSet ds = new DataSet();

            if (Config.isMysql)
            {
                MySqlConnection mysqlGameCon = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_gs));
                MySqlCommand cmd = new MySqlCommand(cmdText, mysqlGameCon);
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    try
                    {
                        mysqlGameCon.Open();
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        Logger.error(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        mysqlGameCon.Close();
                    }
                }
            }
            else
            {
                SqlCommand cmd = new SqlCommand(cmdText, gameCon);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    try
                    {
                        gameCon.Open();
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        Logger.error(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        gameCon.Close();
                    }
                }
            }
            return ds;
        }

        /// <summary>
        /// 执行SQL命令 - 支持MySQL和MSSQL
        /// </summary>
        /// <param name="cmdText">SQL语句</param>
        /// <returns>影响行数</returns>
        public int ExecuteNonQuery(string cmdText)
        {
            int result = 0;

            if (Config.isMysql)
            {
                MySqlConnection mysqlGameCon = new MySqlConnection(Config.GetMySQLConnectionString(Config.mysql_db_gs));
                MySqlCommand cmd = new MySqlCommand(cmdText, mysqlGameCon);
                try
                {
                    mysqlGameCon.Open();
                    result = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.error(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                    mysqlGameCon.Close();
                }
            }
            else
            {
                SqlCommand cmd = new SqlCommand(cmdText, gameCon);
                try
                {
                    gameCon.Open();
                    result = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.error(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                    gameCon.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// 获取在线账号统计 - 支持MySQL和MSSQL
        /// </summary>
        /// <returns></returns>
        public int GetOnlineAccountCount()
        {
            int count = 0;
            string sql;

            if (Config.isMysql)
            {
                sql = "SELECT COUNT(*) FROM account_data WHERE last_login_time > last_logout_time";
            }
            else
            {
                if (Config.newaccountdatabase)
                {
                    sql = "SELECT COUNT(*) FROM [dbo].[account_data] WHERE last_login_time > last_logout_time";
                }
                else
                {
                    sql = "SELECT COUNT(*) FROM [dbo].[user_data] WHERE last_login_time > last_logout_time";
                }
            }

            try
            {
                DataSet ds = ExecuteSelectCmmond(sql);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    count = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                Logger.error("获取在线账号统计失败：" + ex.Message);
            }

            return count;
        }

        /// <summary>
        /// 获取账号信息 - 支持MySQL和MSSQL
        /// </summary>
        /// <param name="accountName">账号名</param>
        /// <returns></returns>
        public DataSet GetAccountInfo(string accountName)
        {
            string sql;

            if (Config.isMysql)
            {
                sql = string.Format("SELECT * FROM account_data WHERE `name` = '{0}'", accountName);
            }
            else
            {
                if (Config.newaccountdatabase)
                {
                    sql = string.Format("SELECT * FROM [dbo].[account_data] WHERE [name] = '{0}'", accountName);
                }
                else
                {
                    sql = string.Format("SELECT * FROM [dbo].[user_auth] WHERE [account] = '{0}'", accountName);
                }
            }

            return ExecuteSelectCmmond(sql);
        }
    }
}
