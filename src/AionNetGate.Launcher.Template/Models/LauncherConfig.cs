namespace AionNetGate.Launcher.Template.Models;

/// <summary>
/// 启动器配置
/// </summary>
public class LauncherConfig
{
    public string GameTitle { get; set; } = "Aion Online";
    public string ServerName { get; set; } = "官方服务器";
    public string Version { get; set; } = "1.0.0";
    public GatewayConfig Gateway { get; set; } = new();
    public GameConfig Game { get; set; } = new();
    public UpdateConfig Update { get; set; } = new();
    public AntiCheatConfig AntiCheat { get; set; } = new();
    public SkinConfig Skin { get; set; } = new();
}

public class GatewayConfig
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 10001;
}

public class GameConfig
{
    public string ExecutablePath { get; set; } = "bin64/aion.bin";
    public string WorkingDirectory { get; set; } = "";
    public string CommandLineArgs { get; set; } = "";
    public int LsPort { get; set; } = 2106;
}

public class UpdateConfig
{
    public string CheckUrl { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
}

public class AntiCheatConfig
{
    public bool Enabled { get; set; } = false;
    public List<string> ProcessBlacklist { get; set; } = new();
    public bool FileIntegrityCheck { get; set; } = false;
}

public class SkinConfig
{
    public string BackgroundImage { get; set; } = "";
    public Dictionary<string, string> ButtonImages { get; set; } = new();
    public bool ShowWebBrowser { get; set; } = false;
    public string WebUrl { get; set; } = "";
}
