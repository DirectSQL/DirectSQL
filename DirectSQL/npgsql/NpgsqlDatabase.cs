using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Npgsql;

namespace DirectSQL.Npgsql
{
    public class NpgsqlDatabase : Database
    {

        private String _npgsqlConnectionString;

        public NpgsqlDatabase( String npgsqlConnectionString)
        {
            _npgsqlConnectionString = npgsqlConnectionString;
        }

        protected override IDbConnection CreateConnection()
        {
            return new NpgsqlConnection( _npgsqlConnectionString );
        }


        public static NpgsqlParameter CreateSQLiteParameter(string name, object value)
        {
            return new NpgsqlParameter(name, value);
        }

    }
}
