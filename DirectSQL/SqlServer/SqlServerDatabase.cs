using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Data.SqlClient;

namespace DirectSQL.SqlServer
{
    public class SqlServerDatabase : Database<SqlConnection, SqlTransaction, SqlCommand, SqlDataReader, SqlParameter>
    {

        readonly private String _sqlServerConnectionString;

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
