using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using MySql.Data.MySqlClient;

namespace DirectSQL.MySql
{
    public class MySqlDatabase : Database
    {

        readonly private String _mySqlConnectionString;

        public MySqlDatabase(String mySqlConnectionString)
        {
            _mySqlConnectionString = mySqlConnectionString;
        }

        protected override IDbConnection CreateConnection()
        {
            return new MySqlConnection( _mySqlConnectionString );
        }


        public static MySqlParameter CreateMySqlParameter(string name, object value)
        {
            return new MySqlParameter(name, value);
        }

    }
}
