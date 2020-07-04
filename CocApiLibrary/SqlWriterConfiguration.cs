using Dapper.SqlWriter;

using System;
using System.Data;
using System.Data.SQLite;

namespace devhl.CocApi
{
    public class SqlWriterConfiguration : ISqlWriterConfiguration
    {
        private readonly string _path;

        public SqlWriterConfiguration(string path)
        {
            _path = path;
        }

        public TimeSpan SlowQueryWarning { get; } = TimeSpan.FromSeconds(5);

        public int? CommandTimeOut { get; }

        public bool AllowConcurrentQueries { get; }

        public IDbConnection CreateDbConnection()
        {
            return new SQLiteConnection(_path);
        }
    }
}