using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using IBM.Data.DB2.Core;

namespace DirectSQL.DB2
{
    public class DB2Database : Database
    {

        readonly private String _db2ConnectionString;

        public DB2Database(String db2ConnectionString)
        {
            _db2ConnectionString = db2ConnectionString;
        }

        protected override IDbConnection CreateConnection()
        {
            return new DB2Connection ( _db2ConnectionString );
        }

        public static DB2Parameter CreateDB2Parameter(string name, object value)
        {
            return new DB2Parameter(name, value);
        }

    }
}
