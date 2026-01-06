namespace AionNetGate.Network.Protocol;

/// <summary>
/// Legacy 数据包加密/解密服务
/// 使用 XOR 加密，密钥为 "煌" (0x714C)
/// 注意：此加密方式安全性较低，仅用于兼容老客户端
/// </summary>
[Obsolete("仅用于兼容老客户端，新客户端请使用 SecureEncryption")]
public static class PacketEncryption
{
    /// <summary>
    /// XOR 加密密钥 - "煌" 的 Unicode 值
    /// </summary>
    private const ushort XorKey = 0x714C;

    /// <summary>
    /// 加密数据（原地修改）
    /// </summary>
    /// <param name="data">要加密的数据</param>
    public static void Encrypt(Span<byte> data)
    {
        // XOR 加密是对称的，加密和解密使用相同的操作
        ApplyXor(data);
    }

    /// <summary>
    /// 解密数据（原地修改）
    /// </summary>
    /// <param name="data">要解密的数据</param>
    public static void Decrypt(Span<byte> data)
    {
        // XOR 解密与加密相同
        ApplyXor(data);
    }

    /// <summary>
    /// 加密数据并返回新数组
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <returns>加密后的数据</returns>
    public static byte[] EncryptToArray(ReadOnlySpan<byte> data)
    {
        var result = new byte[data.Length];
        data.CopyTo(result);
        ApplyXor(result);
        return result;
    }

    /// <summary>
    /// 解密数据并返回新数组
    /// </summary>
    /// <param name="data">加密数据</param>
    /// <returns>解密后的数据</returns>
    public static byte[] DecryptToArray(ReadOnlySpan<byte> data)
    {
        return EncryptToArray(data); // XOR 对称
    }

    /// <summary>
    /// 应用 XOR 操作
    /// </summary>
    private static void ApplyXor(Span<byte> data)
    {
        // 使用低字节进行 XOR（兼容老项目的实现）
        byte keyByte = (byte)(XorKey & 0xFF);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= keyByte;
        }
    }
}
