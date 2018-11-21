using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Data.SQLite;

namespace DirectSQL.SqlLite
{
    public class SqlLiteDatabase : Database
    {

        private String _sqlLiteConnectionString;

        public SqlLiteDatabase( String sqlLiteConnectionString)
        {
            _sqlLiteConnectionString = sqlLiteConnectionString;
        }

        protected override IDbConnection CreateConnection()
        {
            return new SQLiteConnection( _sqlLiteConnectionString );
        }


        protected override IDbDataParameter CreateDbDataParameter(string name, object value)
        {
            return new SQLiteParameter(name, value);
        }
    }
}
