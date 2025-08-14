using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Globalization;

namespace JoyConfig.Infrastructure.Abstract.Abstractions;

/// <summary>
/// 数据库上下文抽象接口
/// </summary>
public interface IDatabaseContext : IDisposable
{
    /// <summary>
    /// 保存更改
    /// </summary>
    /// <returns>受影响的行数</returns>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// 开始事�?    /// </summary>
    /// <returns>事务对象</returns>
    Task<IDbTransaction> BeginTransactionAsync();
    
    /// <summary>
    /// 检查数据库连接
    /// </summary>
    /// <returns>是否可连�?/returns>
    Task<bool> CanConnectAsync();
    
    /// <summary>
    /// 确保数据库已创建
    /// </summary>
    /// <returns>创建任务</returns>
    Task EnsureCreatedAsync();
    
    /// <summary>
    /// 迁移数据�?    /// </summary>
    /// <returns>迁移任务</returns>
    Task MigrateAsync();
}

/// <summary>
/// 数据库事务抽象接�?/// </summary>
public interface IDbTransaction : IDisposable
{
    /// <summary>
    /// 提交事务
    /// </summary>
    /// <returns>提交任务</returns>
    Task CommitAsync();
    
    /// <summary>
    /// 回滚事务
    /// </summary>
    /// <returns>回滚任务</returns>
    Task RollbackAsync();
}

/// <summary>
/// 仓储模式抽象接口
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>实体对象</returns>
    Task<T?> GetByIdAsync(object id);
    
    /// <summary>
    /// 获取所有实�?    /// </summary>
    /// <returns>实体列表</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// 根据条件查找实体
    /// </summary>
    /// <param name="predicate">查找条件</param>
    /// <returns>实体列表</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>添加任务</returns>
    Task AddAsync(T entity);
    
    /// <summary>
    /// 批量添加实体
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>添加任务</returns>
    Task AddRangeAsync(IEnumerable<T> entities);
    
    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>更新任务</returns>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>删除任务</returns>
    Task DeleteAsync(T entity);
    
    /// <summary>
    /// 根据ID删除实体
    /// </summary>
    /// <param name="id">实体ID</param>
    /// <returns>删除任务</returns>
    Task DeleteByIdAsync(object id);
    
    /// <summary>
    /// 检查实体是否存�?    /// </summary>
    /// <param name="predicate">检查条�?/param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// 获取实体数量
    /// </summary>
    /// <param name="predicate">计数条件</param>
    /// <returns>实体数量</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

/// <summary>
/// 工作单元抽象接口
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// 获取仓储
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <returns>仓储实例</returns>
    IRepository<T> GetRepository<T>() where T : class;
    
    /// <summary>
    /// 保存所有更�?    /// </summary>
    /// <returns>受影响的行数</returns>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// 开始事�?    /// </summary>
    /// <returns>事务对象</returns>
    Task<IDbTransaction> BeginTransactionAsync();
}

/// <summary>
/// 缓存服务抽象接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存�?    /// </summary>
    /// <typeparam name="T">值类�?/typeparam>
    /// <param name="key">缓存�?/param>
    /// <returns>缓存�?/returns>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// 设置缓存�?    /// </summary>
    /// <typeparam name="T">值类�?/typeparam>
    /// <param name="key">缓存�?/param>
    /// <param name="value">缓存�?/param>
    /// <param name="expiration">过期时间</param>
    /// <returns>设置任务</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="key">缓存�?/param>
    /// <returns>删除任务</returns>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// 检查缓存是否存�?    /// </summary>
    /// <param name="key">缓存�?/param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// 清空所有缓�?    /// </summary>
    /// <returns>清空任务</returns>
    Task ClearAsync();
}

/// <summary>
/// 外部API服务抽象接口
/// </summary>
public interface IExternalApiService
{
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <typeparam name="T">响应类型</typeparam>
    /// <param name="endpoint">API端点</param>
    /// <param name="headers">请求�?/param>
    /// <returns>响应结果</returns>
    Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null);
    
    /// <summary>
    /// 发送POST请求
    /// </summary>
    /// <typeparam name="TRequest">请求类型</typeparam>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="endpoint">API端点</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求�?/param>
    /// <returns>响应结果</returns>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, Dictionary<string, string>? headers = null);
    
    /// <summary>
    /// 发送PUT请求
    /// </summary>
    /// <typeparam name="TRequest">请求类型</typeparam>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="endpoint">API端点</param>
    /// <param name="data">请求数据</param>
    /// <param name="headers">请求�?/param>
    /// <returns>响应结果</returns>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, Dictionary<string, string>? headers = null);
    
    /// <summary>
    /// 发送DELETE请求
    /// </summary>
    /// <param name="endpoint">API端点</param>
    /// <param name="headers">请求�?/param>
    /// <returns>删除任务</returns>
    Task<bool> DeleteAsync(string endpoint, Dictionary<string, string>? headers = null);
}

/// <summary>
/// 消息队列服务抽象接口
/// </summary>
public interface IMessageQueueService
{
    /// <summary>
    /// 发送消�?    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="queueName">队列名称</param>
    /// <param name="message">消息内容</param>
    /// <returns>发送任�?/returns>
    Task SendAsync<T>(string queueName, T message);
    
    /// <summary>
    /// 接收消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="queueName">队列名称</param>
    /// <returns>消息内容</returns>
    Task<T?> ReceiveAsync<T>(string queueName);
    
    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="queueName">队列名称</param>
    /// <param name="handler">消息处理�?/param>
    /// <returns>订阅任务</returns>
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler);
    
    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <param name="queueName">队列名称</param>
    /// <returns>取消订阅任务</returns>
    Task UnsubscribeAsync(string queueName);
}

/// <summary>
/// 邮件服务抽象接口
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// 发送邮�?    /// </summary>
    /// <param name="to">收件�?/param>
    /// <param name="subject">主题</param>
    /// <param name="body">邮件内容</param>
    /// <param name="isHtml">是否HTML格式</param>
    /// <returns>发送任�?/returns>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    
    /// <summary>
    /// 发送邮件（多个收件人）
    /// </summary>
    /// <param name="recipients">收件人列�?/param>
    /// <param name="subject">主题</param>
    /// <param name="body">邮件内容</param>
    /// <param name="isHtml">是否HTML格式</param>
    /// <returns>发送任�?/returns>
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = false);
    
    /// <summary>
    /// 发送带附件的邮�?    /// </summary>
    /// <param name="to">收件�?/param>
    /// <param name="subject">主题</param>
    /// <param name="body">邮件内容</param>
    /// <param name="attachments">附件路径列表</param>
    /// <param name="isHtml">是否HTML格式</param>
    /// <returns>发送任�?/returns>
    Task SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<string> attachments, bool isHtml = false);
}
