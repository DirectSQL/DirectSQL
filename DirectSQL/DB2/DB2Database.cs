using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using IBM.Data.DB2.Core;

namespace DirectSQL.DB2
{
    public class DB2Database : Database<DB2Connection, DB2Transaction, DB2Command, DB2DataReader, DB2Parameter>
    {

        readonly private String _db2ConnectionString;

        public DB2Database(String db2ConnectionString)
        {
            _db2ConnectionString = db2ConnectionString;
        }

        protected override DB2Connection CreateConnection()
        {
            return new DB2Connection ( _db2ConnectionString );
        }

    }
}
