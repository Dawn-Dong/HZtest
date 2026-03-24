using System;
using System.Collections.Generic;
using System.Text;

namespace HZtest.Infrastructure_基础设施.Settings
{
    public class DbConnectionSettings
    {
        // 基础字段（所有数据库通用）
        public string ConnectionString { get; set; }
        public int DbType { get; set; }
        public string ConfigId { get; set; }
        public bool IsAutoCloseConnection { get; set; }

        // 扩展字段（SQLite）
        public string Path { get; set; }
        public string FileName { get; set; }

        // 扩展字段（MySQL等）
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class SqlSugarAppSettings
    {
        public List<DbConnectionSettings> ConnectionConfigs { get; set; }
    }
}
