namespace AionNetGate.Core.Interfaces;

/// <summary>
/// 泛型仓储接口，定义基础的 CRUD 操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// 根据 ID 获取实体
    /// </summary>
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加实体
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新实体
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除实体
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 删除实体
    /// </summary>
    Task DeleteByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查实体是否存在
    /// </summary>
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取实体数量
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据条件获取实体数量
    /// </summary>
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>>? filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询
    /// </summary>
    Task<IEnumerable<TEntity>> GetPagedAsync(
        int page,
        int pageSize,
        System.Linq.Expressions.Expression<Func<TEntity, bool>>? filter = null,
        string? orderBy = null,
        bool descending = true,
        CancellationToken cancellationToken = default);
}
