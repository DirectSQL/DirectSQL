using System;

using Microsoft.Data.SqlClient;

namespace DirectSQL.SqlServer
{
    public class SqlServerDatabase : Database<SqlConnection, SqlTransaction, SqlCommand, SqlDataReader, SqlParameter>
    {
        private readonly String _sqlServerConnectionString;

        public SqlServerDatabase(String sqlServerConnectionString)
        {
            _sqlServerConnectionString = sqlServerConnectionString;
        }

        protected override SqlConnection CreateConnection()
        {
            return new SqlConnection ( _sqlServerConnectionString );
        }
    }
}
