using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Data.SQLite;

namespace DirectSQL.SqlLite
{
    public class SqlLiteDatabase : Database<SQLiteConnection, SQLiteTransaction, SQLiteCommand, SQLiteDataReader, SQLiteParameter>
    {
        readonly private String _sqlLiteConnectionString;

        public SqlLiteDatabase(String sqlLiteConnectionString)
        {
            _sqlLiteConnectionString = sqlLiteConnectionString;
        }

        protected override SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection( _sqlLiteConnectionString );
        }
    }
}
