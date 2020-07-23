using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using MySql.Data.MySqlClient;

namespace DirectSQL.MySql
{
    public class MySqlDatabase : Database<MySqlConnection, MySqlTransaction, MySqlCommand, MySqlDataReader, MySqlParameter>
    {
        private readonly String _mySqlConnectionString;

        public MySqlDatabase(String mySqlConnectionString)
        {
            _mySqlConnectionString = mySqlConnectionString;
        }

        protected override MySqlConnection CreateConnection()
        {
            return new MySqlConnection( _mySqlConnectionString );
        }
    }
}
