using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AionGate.Updater;

/// <summary>
/// CDN URL签名生成器 - 生成带时效性的下载URL
/// 支持: 阿里云OSS、腾讯云COS、AWS S3、Cloudflare R2
/// </summary>
public class CDNUrlSigner
{
    private readonly CDNConfig _config;

    public CDNUrlSigner(CDNConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// 生成签名URL
    /// </summary>
    public string GenerateSignedUrl(string objectKey, int expiresInMinutes = 60)
    {
        return _config.Provider switch
        {
            CDNProvider.AliOSS => GenerateAliOSSSignedUrl(objectKey, expiresInMinutes),
            CDNProvider.TencentCOS => GenerateTencentCOSSignedUrl(objectKey, expiresInMinutes),
            CDNProvider.AWSS3 => GenerateAWSS3SignedUrl(objectKey, expiresInMinutes),
            CDNProvider.CloudflareR2 => GenerateCloudflareR2SignedUrl(objectKey, expiresInMinutes),
            _ => throw new NotSupportedException($"不支持的CDN提供商: {_config.Provider}")
        };
    }

    /// <summary>
    /// 阿里云OSS签名URL (支持CDN加速)
    /// </summary>
    private string GenerateAliOSSSignedUrl(string objectKey, int expiresInMinutes)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes).ToUnixTimeSeconds();
        var resource = $"/{_config.BucketName}/{objectKey}";

        // 构建签名字符串
        var stringToSign = $"GET\n\n\n{expires}\n{resource}";
        var signature = SignHmacSHA1(stringToSign, _config.SecretKey);

        // 使用CDN域名（如果配置了）
        var domain = !string.IsNullOrEmpty(_config.CDNDomain)
            ? _config.CDNDomain
            : $"{_config.BucketName}.{_config.Endpoint}";

        var signedUrl = $"https://{domain}/{objectKey}?" +
                       $"OSSAccessKeyId={_config.AccessKey}&" +
                       $"Expires={expires}&" +
                       $"Signature={Uri.EscapeDataString(signature)}";

        return signedUrl;
    }

    /// <summary>
    /// 腾讯云COS签名URL
    /// </summary>
    private string GenerateTencentCOSSignedUrl(string objectKey, int expiresInMinutes)
    {
        var startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var endTime = startTime + (expiresInMinutes * 60);

        // 生成随机字符串
        var random = Guid.NewGuid().ToString("N").Substring(0, 10);

        // 构建签名原串
        var signTime = $"{startTime};{endTime}";
        var keyTime = signTime;
        var urlParamList = string.Empty;
        var headerList = "host";

        var domain = !string.IsNullOrEmpty(_config.CDNDomain)
            ? _config.CDNDomain
            : $"{_config.BucketName}.cos.{_config.Region}.myqcloud.com";

        var httpString = $"get\n/{objectKey}\n\nhost={domain}\n";
        var stringToSign = $"sha1\n{keyTime}\n{Sha1(httpString)}\n";
        var signKey = SignHmacSHA1(keyTime, _config.SecretKey);
        var signature = SignHmacSHA1(stringToSign, signKey);

        var authorization = $"q-sign-algorithm=sha1&" +
                          $"q-ak={_config.AccessKey}&" +
                          $"q-sign-time={signTime}&" +
                          $"q-key-time={keyTime}&" +
                          $"q-header-list={headerList}&" +
                          $"q-url-param-list={urlParamList}&" +
                          $"q-signature={signature}";

        return $"https://{domain}/{objectKey}?sign={Uri.EscapeDataString(authorization)}";
    }

    /// <summary>
    /// AWS S3签名URL (Signature Version 4)
    /// </summary>
    private string GenerateAWSS3SignedUrl(string objectKey, int expiresInMinutes)
    {
        var now = DateTime.UtcNow;
        var amzDate = now.ToString("yyyyMMddTHHmmssZ");
        var dateStamp = now.ToString("yyyyMMdd");
        var expiresInSeconds = expiresInMinutes * 60;

        var credentialScope = $"{dateStamp}/{_config.Region}/s3/aws4_request";
        var domain = !string.IsNullOrEmpty(_config.CDNDomain)
            ? _config.CDNDomain
            : $"{_config.BucketName}.s3.{_config.Region}.amazonaws.com";

        // 构建规范请求
        var canonicalQueryString =
            $"X-Amz-Algorithm=AWS4-HMAC-SHA256&" +
            $"X-Amz-Credential={Uri.EscapeDataString($"{_config.AccessKey}/{credentialScope}")}&" +
            $"X-Amz-Date={amzDate}&" +
            $"X-Amz-Expires={expiresInSeconds}&" +
            $"X-Amz-SignedHeaders=host";

        var canonicalRequest =
            $"GET\n" +
            $"/{objectKey}\n" +
            $"{canonicalQueryString}\n" +
            $"host:{domain}\n\n" +
            $"host\n" +
            $"UNSIGNED-PAYLOAD";

        var stringToSign =
            $"AWS4-HMAC-SHA256\n" +
            $"{amzDate}\n" +
            $"{credentialScope}\n" +
            $"{Sha256(canonicalRequest)}";

        // 计算签名
        var kDate = SignHmacSHA256($"AWS4{_config.SecretKey}", dateStamp);
        var kRegion = SignHmacSHA256(kDate, _config.Region);
        var kService = SignHmacSHA256(kRegion, "s3");
        var kSigning = SignHmacSHA256(kService, "aws4_request");
        var signature = SignHmacSHA256Hex(kSigning, stringToSign);

        return $"https://{domain}/{objectKey}?{canonicalQueryString}&X-Amz-Signature={signature}";
    }

    /// <summary>
    /// Cloudflare R2签名URL (兼容S3 API)
    /// </summary>
    private string GenerateCloudflareR2SignedUrl(string objectKey, int expiresInMinutes)
    {
        // R2使用S3兼容API，但endpoint不同
        var originalEndpoint = _config.Endpoint;
        _config.Endpoint = $"{_config.AccountId}.r2.cloudflarestorage.com";

        var signedUrl = GenerateAWSS3SignedUrl(objectKey, expiresInMinutes);

        _config.Endpoint = originalEndpoint;
        return signedUrl;
    }

    /// <summary>
    /// HMAC-SHA1签名
    /// </summary>
    private static string SignHmacSHA1(string data, string key)
    {
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// HMAC-SHA256签名（返回字节）
    /// </summary>
    private static byte[] SignHmacSHA256(string key, string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// HMAC-SHA256签名（返回字节）
    /// </summary>
    private static byte[] SignHmacSHA256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// HMAC-SHA256签名（返回十六进制字符串）
    /// </summary>
    private static string SignHmacSHA256Hex(byte[] key, string data)
    {
        var hash = SignHmacSHA256(key, data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// SHA1哈希
    /// </summary>
    private static string Sha1(string data)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// SHA256哈希
    /// </summary>
    private static string Sha256(string data)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

/// <summary>
/// CDN配置
/// </summary>
public class CDNConfig
{
    /// <summary>
    /// CDN提供商
    /// </summary>
    public CDNProvider Provider { get; set; }

    /// <summary>
    /// 访问密钥ID
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// 访问密钥Secret
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 存储桶名称
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// 区域
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// 终端节点
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// CDN加速域名（可选）
    /// </summary>
    public string? CDNDomain { get; set; }

    /// <summary>
    /// Cloudflare账号ID (仅R2需要)
    /// </summary>
    public string? AccountId { get; set; }
}

/// <summary>
/// CDN提供商
/// </summary>
public enum CDNProvider
{
    /// <summary>
    /// 阿里云对象存储OSS
    /// </summary>
    AliOSS,

    /// <summary>
    /// 腾讯云对象存储COS
    /// </summary>
    TencentCOS,

    /// <summary>
    /// AWS S3
    /// </summary>
    AWSS3,

    /// <summary>
    /// Cloudflare R2
    /// </summary>
    CloudflareR2
}
