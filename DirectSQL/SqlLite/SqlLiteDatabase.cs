using System;

using System.Data.SQLite;

namespace DirectSQL.SqlLite
{
    public class SqlLiteDatabase : Database<SQLiteConnection, SQLiteTransaction, SQLiteCommand, SQLiteDataReader, SQLiteParameter>
    {
        private readonly String _sqlLiteConnectionString;

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
