using HZtest.Infrastructure_基础设施.Settings;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HZtest.Infrastructure_基础设施
{
    /// <summary>
    ///  SqlSugar 数据库连接配置
    /// </summary>
    public class SqlSugarSetup
    {
        /// <summary>
        /// 创建 SqlSugar 数据库连接
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static SqlSugarClient CreateClient(IConfiguration config)
        {
            // 用自定义类读取配置
            var settings = config.GetSection("ConnectionConfigs")
                .Get<List<DbConnectionSettings>>();

            // 转换为 SqlSugar 的 ConnectionConfig
            var sugarConfigs = settings?.Select(s => new ConnectionConfig
            {
                ConfigId = s.ConfigId,
                DbType = (DbType)s.DbType,
                ConnectionString = BuildConnectionString(s),
                IsAutoCloseConnection = s.IsAutoCloseConnection
            }).ToList();

            return new SqlSugarClient(sugarConfigs);
        }

        /// <summary>
        /// 连接字符串构建器，根据不同的数据库类型进行拼接
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string BuildConnectionString(DbConnectionSettings s)
        {
            return s.ConfigId switch
            {
                "SQLite" => BuildSQLiteString(s),
                "MySQL" => BuildMySQLString(s),
                _ => s.ConnectionString
            };
        }

        /// <summary>
        /// 拼接 SQLite 连接字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string BuildSQLiteString(DbConnectionSettings s)
        {
            //获取程序所在目录：D:\Code\HZtest\HZtest\bin\Debug\net10.0-windows\ 并且替换掉路径中的占位符 {LocalAppData}
#if DEBUG
            var path = s.Path.Replace("{BaseDir}", AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\");
            System.Diagnostics.Debug.WriteLine($"Debug SQLite 数据库路径：{path}");
#else
           var path = s.Path.Replace("{BaseDir}", AppDomain.CurrentDomain.BaseDirectory).Replace("/", "\\");
           System.Diagnostics.Debug.WriteLine($"Release SQLite 数据库路径：{path}");
#endif
            Directory.CreateDirectory(path);

            var fullPath = Path.Combine(path, s.FileName);
            return string.Format(s.ConnectionString, fullPath);
        }
        /// <summary>
        /// 拼接 MySQL 连接字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string BuildMySQLString(DbConnectionSettings s)
        {
            return string.Format(s.ConnectionString, s.Host, s.Port, s.Database, s.User, s.Password);
        }
    }
}
