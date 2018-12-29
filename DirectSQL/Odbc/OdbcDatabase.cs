using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.Data.Odbc;

namespace DirectSQL.OdbcServer
{
    public class OdbcDatabase : Database<OdbcConnection, OdbcTransaction, OdbcCommand, OdbcDataReader, OdbcParameter>
    {

        readonly private String _odbcConnectionString;

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
