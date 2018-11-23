using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Data.SqlClient;

namespace DirectSQL.SqlServer
{
    public class SqlServerDatabase : Database
    {

        readonly private String _sqlServerConnectionString;

        public SqlServerDatabase(String sqlServerConnectionString)
        {
            _sqlServerConnectionString = sqlServerConnectionString;
        }

        protected override IDbConnection CreateConnection()
        {
            return new SqlConnection ( _sqlServerConnectionString );
        }

        public static SqlParameter CreateSqlParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }

    }
}
