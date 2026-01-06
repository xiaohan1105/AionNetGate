namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 加密服务接口
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// 加密数据
    /// </summary>
    /// <param name="plaintext">明文数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>密文数据</returns>
    Task<byte[]> EncryptAsync(ReadOnlyMemory<byte> plaintext, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解密数据
    /// </summary>
    /// <param name="ciphertext">密文数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>明文数据</returns>
    Task<byte[]> DecryptAsync(ReadOnlyMemory<byte> ciphertext, CancellationToken cancellationToken = default);

    /// <summary>
    /// 加密数据（同步版本）
    /// </summary>
    /// <param name="plaintext">明文数据</param>
    /// <returns>密文数据</returns>
    byte[] Encrypt(ReadOnlySpan<byte> plaintext);

    /// <summary>
    /// 解密数据（同步版本）
    /// </summary>
    /// <param name="ciphertext">密文数据</param>
    /// <returns>明文数据</returns>
    byte[] Decrypt(ReadOnlySpan<byte> ciphertext);
}
