using System.Data.Common;
using AionNetGate.Admin.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Microsoft.Data.SqlClient;

namespace AionNetGate.Admin.WPF.ViewModels;

/// <summary>
/// 数据库配置页面 ViewModel
/// </summary>
public partial class DatabaseConfigViewModel : ViewModelBase
{
    private readonly ILogger<DatabaseConfigViewModel> _logger;
    private readonly IConfigurationService _configService;
    private const string ConfigName = "database";

    [ObservableProperty]
    private bool _isMySql = true;

    [ObservableProperty]
    private bool _isMsSql;

    [ObservableProperty]
    private string _serverAddress = "localhost";

    [ObservableProperty]
    private int _serverPort = 3306;

    [ObservableProperty]
    private string _username = "root";

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _accountDatabase = "aion_ls";

    [ObservableProperty]
    private string _gameDatabase = "aion_gs";

    [ObservableProperty]
    private string _charset = "utf8mb4";

    [ObservableProperty]
    private bool _enableConnectionPooling = true;

    [ObservableProperty]
    private int _connectionTimeout = 30;

    [ObservableProperty]
    private bool _enableDatabaseLogging;

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private bool _testSuccess;

    public DatabaseConfigViewModel(
        ILogger<DatabaseConfigViewModel> logger,
        IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
        LoadConfig();
        _logger.LogInformation("DatabaseConfigViewModel 已初始化");
    }

    private void LoadConfig()
    {
        var config = _configService.LoadConfig<DatabaseConfigData>(ConfigName);
        if (config != null)
        {
            IsMySql = config.IsMySql;
            IsMsSql = config.IsMsSql;
            ServerAddress = config.ServerAddress;
            ServerPort = config.ServerPort;
            Username = config.Username;
            Password = config.Password;
            AccountDatabase = config.AccountDatabase;
            GameDatabase = config.GameDatabase;
            Charset = config.Charset;
            EnableConnectionPooling = config.EnableConnectionPooling;
            ConnectionTimeout = config.ConnectionTimeout;
            EnableDatabaseLogging = config.EnableDatabaseLogging;
            StatusText = "配置已加载";
        }
    }

    partial void OnIsMySqlChanged(bool value)
    {
        if (value)
        {
            IsMsSql = false;
            ServerPort = 3306;
        }
    }

    partial void OnIsMsSqlChanged(bool value)
    {
        if (value)
        {
            IsMySql = false;
            ServerPort = 1433;
        }
    }

    private string BuildConnectionString()
    {
        if (IsMySql)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = ServerAddress,
                Port = (uint)ServerPort,
                UserID = Username,
                Password = Password,
                Database = AccountDatabase,
                CharacterSet = Charset,
                ConnectionTimeout = (uint)ConnectionTimeout,
                Pooling = EnableConnectionPooling
            };
            return builder.ConnectionString;
        }
        else
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = $"{ServerAddress},{ServerPort}",
                UserID = Username,
                Password = Password,
                InitialCatalog = AccountDatabase,
                ConnectTimeout = ConnectionTimeout,
                Pooling = EnableConnectionPooling,
                TrustServerCertificate = true
            };
            return builder.ConnectionString;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsTesting) return;

        try
        {
            IsTesting = true;
            TestSuccess = false;
            StatusText = "正在测试连接...";

            var connectionString = BuildConnectionString();
            DbConnection connection = IsMySql
                ? new MySqlConnection(connectionString)
                : new SqlConnection(connectionString);

            await using (connection)
            {
                await connection.OpenAsync();

                // 执行简单查询验证连接
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = IsMySql ? "SELECT VERSION()" : "SELECT @@VERSION";
                var version = await cmd.ExecuteScalarAsync();

                TestSuccess = true;
                StatusText = $"连接成功！服务器版本: {version?.ToString()?.Split('\n')[0]}";
                _logger.LogInformation("数据库连接测试成功: {Version}", version);
            }
        }
        catch (Exception ex)
        {
            TestSuccess = false;
            StatusText = "连接失败: " + ex.Message;
            _logger.LogError(ex, "数据库连接测试失败");
        }
        finally
        {
            IsTesting = false;
        }
    }

    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            var config = new DatabaseConfigData
            {
                IsMySql = IsMySql,
                IsMsSql = IsMsSql,
                ServerAddress = ServerAddress,
                ServerPort = ServerPort,
                Username = Username,
                Password = Password,
                AccountDatabase = AccountDatabase,
                GameDatabase = GameDatabase,
                Charset = Charset,
                EnableConnectionPooling = EnableConnectionPooling,
                ConnectionTimeout = ConnectionTimeout,
                EnableDatabaseLogging = EnableDatabaseLogging
            };

            _configService.SaveConfig(ConfigName, config);
            StatusText = "配置已保存";
            _logger.LogInformation("数据库配置已保存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败");
            StatusText = "保存失败: " + ex.Message;
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        IsMySql = true;
        IsMsSql = false;
        ServerAddress = "localhost";
        ServerPort = 3306;
        Username = "root";
        Password = string.Empty;
        AccountDatabase = "aion_ls";
        GameDatabase = "aion_gs";
        Charset = "utf8mb4";
        EnableConnectionPooling = true;
        ConnectionTimeout = 30;
        EnableDatabaseLogging = false;
        StatusText = "已恢复默认配置";
    }
}

/// <summary>
/// 数据库配置数据模型
/// </summary>
public class DatabaseConfigData
{
    public bool IsMySql { get; set; } = true;
    public bool IsMsSql { get; set; }
    public string ServerAddress { get; set; } = "localhost";
    public int ServerPort { get; set; } = 3306;
    public string Username { get; set; } = "root";
    public string Password { get; set; } = string.Empty;
    public string AccountDatabase { get; set; } = "aion_ls";
    public string GameDatabase { get; set; } = "aion_gs";
    public string Charset { get; set; } = "utf8mb4";
    public bool EnableConnectionPooling { get; set; } = true;
    public int ConnectionTimeout { get; set; } = 30;
    public bool EnableDatabaseLogging { get; set; }
}
