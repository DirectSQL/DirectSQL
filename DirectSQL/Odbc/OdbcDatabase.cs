using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Data.Odbc;

namespace DirectSQL.Odbc
{
    public class OdbcDatabase : Database<OdbcConnection, OdbcTransaction, OdbcCommand, OdbcDataReader, OdbcParameter>
    {
        private readonly String _odbcConnectionString;

        public OdbcDatabase(String odbcConnectionString)
        {
            _odbcConnectionString = odbcConnectionString;
        }

        protected override OdbcConnection CreateConnection()
        {
            return new OdbcConnection ( _odbcConnectionString );
        }
    }
}
