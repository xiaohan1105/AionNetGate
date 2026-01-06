namespace AionNetGate.Network.Models;

/// <summary>
/// 远程桌面数据
/// </summary>
public sealed class RemoteDesktopData
{
    /// <summary>屏幕宽度</summary>
    public int Width { get; init; }

    /// <summary>屏幕高度</summary>
    public int Height { get; init; }

    /// <summary>图像块列表</summary>
    public List<ImageBlock> Blocks { get; init; } = [];

    /// <summary>压缩率 (0-100)</summary>
    public int CompressionRate { get; init; } = 50;

    /// <summary>最大尺寸限制</summary>
    public const int MaxWidth = 4096;
    public const int MaxHeight = 4096;
    public const int MaxBlockCount = 1000;
    public const int MaxBlockSize = 1024 * 1024; // 1MB
}

/// <summary>
/// 图像块
/// </summary>
public sealed class ImageBlock
{
    /// <summary>X 偏移</summary>
    public int X { get; init; }

    /// <summary>Y 偏移</summary>
    public int Y { get; init; }

    /// <summary>块宽度</summary>
    public int Width { get; init; }

    /// <summary>块高度</summary>
    public int Height { get; init; }

    /// <summary>图像数据（JPEG/PNG 格式）</summary>
    public byte[] Data { get; init; } = [];
}

/// <summary>
/// 远程进程信息
/// </summary>
public sealed class RemoteProcessInfo
{
    /// <summary>进程 ID</summary>
    public int ProcessId { get; init; }

    /// <summary>进程名称</summary>
    public string ProcessName { get; init; } = "";

    /// <summary>窗口标题</summary>
    public string WindowTitle { get; init; } = "";

    /// <summary>内存使用量（字节）</summary>
    public long MemoryUsage { get; init; }

    /// <summary>CPU 使用率 (%)</summary>
    public double CpuUsage { get; init; }

    /// <summary>进程路径</summary>
    public string FilePath { get; init; } = "";

    /// <summary>进程图标（PNG 格式）</summary>
    public byte[]? Icon { get; init; }

    /// <summary>最大进程数量限制</summary>
    public const int MaxProcessCount = 1000;

    /// <summary>最大图标大小</summary>
    public const int MaxIconSize = 100 * 1024; // 100KB
}

/// <summary>
/// 进程操作类型
/// </summary>
public enum ProcessOperationType : byte
{
    /// <summary>获取进程列表</summary>
    List = 0,

    /// <summary>结束进程</summary>
    Kill = 1
}

/// <summary>
/// 远程计算机信息
/// </summary>
public sealed class RemoteComputerInfo
{
    /// <summary>操作系统名称</summary>
    public string OsName { get; init; } = "";

    /// <summary>系统类型 (32位/64位)</summary>
    public string SystemType { get; init; } = "";

    /// <summary>计算机名</summary>
    public string ComputerName { get; init; } = "";

    /// <summary>当前用户名</summary>
    public string UserName { get; init; } = "";

    /// <summary>CPU 信息</summary>
    public string CpuInfo { get; init; } = "";

    /// <summary>内存信息</summary>
    public string MemoryInfo { get; init; } = "";

    /// <summary>显卡信息</summary>
    public string VideoCardInfo { get; init; } = "";

    /// <summary>驱动器信息</summary>
    public string DriveInfo { get; init; } = "";

    /// <summary>主板信息</summary>
    public string MainBoardInfo { get; init; } = "";

    /// <summary>MAC 地址</summary>
    public string MacAddress { get; init; } = "";

    /// <summary>IP 地址</summary>
    public string IpAddress { get; init; } = "";

    /// <summary>地理位置</summary>
    public string Location { get; init; } = "";

    /// <summary>收集时间</summary>
    public DateTime CollectedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 文件系统条目
/// </summary>
public sealed class FileSystemEntry
{
    /// <summary>名称</summary>
    public string Name { get; init; } = "";

    /// <summary>完整路径</summary>
    public string FullPath { get; init; } = "";

    /// <summary>是否为目录</summary>
    public bool IsDirectory { get; init; }

    /// <summary>文件大小（字节）</summary>
    public long Size { get; init; }

    /// <summary>最后修改时间</summary>
    public DateTime LastModified { get; init; }

    /// <summary>文件属性</summary>
    public string Attributes { get; init; } = "";

    /// <summary>文件图标（PNG 格式）</summary>
    public byte[]? Icon { get; init; }

    /// <summary>最大条目数量</summary>
    public const int MaxEntryCount = 10000;

    /// <summary>最大驱动器数量</summary>
    public const int MaxDriveCount = 26;
}

/// <summary>
/// 文件操作类型
/// </summary>
public enum FileOperationType : byte
{
    /// <summary>显示驱动器列表</summary>
    ShowDrives = 0,

    /// <summary>显示文件和目录</summary>
    ShowFilesAndDirs = 1,

    /// <summary>复制文件/目录</summary>
    Copy = 2,

    /// <summary>删除文件/目录</summary>
    Delete = 3,

    /// <summary>下载文件</summary>
    Download = 4,

    /// <summary>上传文件</summary>
    Upload = 5,

    /// <summary>新建文件夹</summary>
    NewFolder = 6,

    /// <summary>重命名</summary>
    Rename = 7,

    /// <summary>执行文件</summary>
    Execute = 8,

    /// <summary>修改文件日期</summary>
    ChangeDate = 9,

    /// <summary>新建文件</summary>
    NewFile = 10
}

/// <summary>
/// 文件操作请求
/// </summary>
public sealed class FileOperationRequest
{
    /// <summary>操作类型</summary>
    public FileOperationType Operation { get; init; }

    /// <summary>路径</summary>
    public string Path { get; init; } = "";

    /// <summary>目标路径（用于复制/移动/重命名）</summary>
    public string? TargetPath { get; init; }

    /// <summary>文件数据（用于上传）</summary>
    public byte[]? Data { get; init; }

    /// <summary>新日期（用于修改日期）</summary>
    public DateTime? NewDate { get; init; }
}

/// <summary>
/// 文件操作结果
/// </summary>
public sealed class FileOperationResult
{
    /// <summary>是否成功</summary>
    public bool Success { get; init; }

    /// <summary>消息</summary>
    public string Message { get; init; } = "";

    /// <summary>驱动器列表（ShowDrives 操作）</summary>
    public List<DriveEntry>? Drives { get; init; }

    /// <summary>文件/目录列表（ShowFilesAndDirs 操作）</summary>
    public List<FileSystemEntry>? Entries { get; init; }

    /// <summary>文件数据（Download 操作）</summary>
    public byte[]? Data { get; init; }
}

/// <summary>
/// 驱动器条目
/// </summary>
public sealed class DriveEntry
{
    /// <summary>驱动器名称（如 C:）</summary>
    public string Name { get; init; } = "";

    /// <summary>驱动器类型</summary>
    public string DriveType { get; init; } = "";

    /// <summary>卷标</summary>
    public string VolumeLabel { get; init; } = "";

    /// <summary>总大小（字节）</summary>
    public long TotalSize { get; init; }

    /// <summary>可用空间（字节）</summary>
    public long FreeSpace { get; init; }

    /// <summary>文件系统（NTFS/FAT32 等）</summary>
    public string FileSystem { get; init; } = "";

    /// <summary>是否就绪</summary>
    public bool IsReady { get; init; }
}

/// <summary>
/// 远程注册表条目
/// </summary>
public sealed class RegistryEntry
{
    /// <summary>键名</summary>
    public string Name { get; init; } = "";

    /// <summary>完整路径</summary>
    public string FullPath { get; init; } = "";

    /// <summary>值类型</summary>
    public RegistryValueType ValueType { get; init; }

    /// <summary>值数据</summary>
    public string? ValueData { get; init; }

    /// <summary>是否为键（否则为值）</summary>
    public bool IsKey { get; init; }

    /// <summary>子键列表</summary>
    public List<string>? SubKeys { get; init; }

    /// <summary>值列表</summary>
    public List<RegistryEntry>? Values { get; init; }
}

/// <summary>
/// 注册表值类型
/// </summary>
public enum RegistryValueType : byte
{
    /// <summary>字符串</summary>
    String = 0,

    /// <summary>可扩展字符串</summary>
    ExpandString = 1,

    /// <summary>二进制</summary>
    Binary = 2,

    /// <summary>32 位整数</summary>
    DWord = 3,

    /// <summary>64 位整数</summary>
    QWord = 4,

    /// <summary>多字符串</summary>
    MultiString = 5,

    /// <summary>未知类型</summary>
    Unknown = 255
}

/// <summary>
/// 注册表操作类型
/// </summary>
public enum RegistryOperationType : byte
{
    /// <summary>获取键和值</summary>
    List = 0,

    /// <summary>创建键</summary>
    CreateKey = 1,

    /// <summary>删除键</summary>
    DeleteKey = 2,

    /// <summary>设置值</summary>
    SetValue = 3,

    /// <summary>删除值</summary>
    DeleteValue = 4
}

/// <summary>
/// 远程服务信息
/// </summary>
public sealed class RemoteServiceInfo
{
    /// <summary>服务名称</summary>
    public string ServiceName { get; init; } = "";

    /// <summary>显示名称</summary>
    public string DisplayName { get; init; } = "";

    /// <summary>服务状态</summary>
    public ServiceStatus Status { get; init; }

    /// <summary>启动类型</summary>
    public ServiceStartType StartType { get; init; }

    /// <summary>服务路径</summary>
    public string? PathName { get; init; }

    /// <summary>描述</summary>
    public string? Description { get; init; }
}

/// <summary>
/// 服务状态
/// </summary>
public enum ServiceStatus : byte
{
    /// <summary>已停止</summary>
    Stopped = 0,

    /// <summary>正在启动</summary>
    StartPending = 1,

    /// <summary>正在停止</summary>
    StopPending = 2,

    /// <summary>正在运行</summary>
    Running = 3,

    /// <summary>继续挂起</summary>
    ContinuePending = 4,

    /// <summary>暂停挂起</summary>
    PausePending = 5,

    /// <summary>已暂停</summary>
    Paused = 6,

    /// <summary>未知</summary>
    Unknown = 255
}

/// <summary>
/// 服务启动类型
/// </summary>
public enum ServiceStartType : byte
{
    /// <summary>自动</summary>
    Automatic = 0,

    /// <summary>手动</summary>
    Manual = 1,

    /// <summary>禁用</summary>
    Disabled = 2,

    /// <summary>延迟启动</summary>
    DelayedAutomatic = 3,

    /// <summary>未知</summary>
    Unknown = 255
}

/// <summary>
/// 服务操作类型
/// </summary>
public enum ServiceOperationType : byte
{
    /// <summary>获取服务列表</summary>
    List = 0,

    /// <summary>启动服务</summary>
    Start = 1,

    /// <summary>停止服务</summary>
    Stop = 2,

    /// <summary>重启服务</summary>
    Restart = 3,

    /// <summary>删除服务</summary>
    Delete = 4
}
