using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Npgsql;

namespace DirectSQL.Npgsql
{
    public class NpgsqlDatabase : Database<NpgsqlConnection, NpgsqlTransaction, NpgsqlCommand, NpgsqlDataReader, NpgsqlParameter>
    {
        private readonly String _npgsqlConnectionString;

        public NpgsqlDatabase(String npgsqlConnectionString)
        {
            _npgsqlConnectionString = npgsqlConnectionString;
        }

        protected override NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection( _npgsqlConnectionString );
        }
    }
}
