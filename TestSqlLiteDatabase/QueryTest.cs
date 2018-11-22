using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;

using System.Data;

using DirectSQL.SqlLite;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class QueryTest
    {
        [TestMethod]
        public void TestQuery()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((connection, transaction) =>
            {
                CreateTableForTest(connection);

                SqlLiteDatabase.ExecuteNonQuery(
                    "insert into TEST_TABLE(TEST_VAL1,TEST_VAL2) values('abcdef',123)",
                    connection,
                    transaction);

                SqlLiteDatabase.Query(
                    "select TEST_VAL1,TEST_VAL2 from TEST_TABLE",
                    connection,
                    transaction,
                    (result) => {
                        if (result.Next())
                        {
                            var resultValues = result.ResultValues();
                            Assert.AreEqual(resultValues.TEST_VAL1, "abcdef");
                            Assert.AreEqual(resultValues.TEST_VAL2, 123);
                        }
                        else
                        {
                            Assert.Fail();
                        }
                    }
                );

                SqlLiteDatabase.Query(
                    "select TEST_VAL1,TEST_VAL2 from TEST_TABLE where TEST_VAL1 = @val1",
                    new IDbDataParameter[] { db.CreateDbDataParameter("@val1","abcdef")},
                    connection,
                    transaction,
                    (result) => {
                        if (result.Next())
                        {
                            var resultValues = result.ResultValues();
                            Assert.AreEqual(resultValues.TEST_VAL1, "abcdef");
                            Assert.AreEqual(resultValues.TEST_VAL2, 123);
                        }
                        else
                        {
                            Assert.Fail();
                        }
                    }
                );

                SqlLiteDatabase.Query(
                    "select TEST_VAL1,TEST_VAL2 from TEST_TABLE where TEST_VAL1 = @val1",
                    new IDbDataParameter[] { db.CreateDbDataParameter("@val1", "XXXXXXX") },
                    connection,
                    transaction,
                    (result) => {
                        Assert.IsFalse(result.Next());
                    }
                );

            });
        }

        private static void CreateTableForTest(IDbConnection connection)
        {
            using( var command = connection.CreateCommand())
            {
                command.CommandText =
                    "create table " +
                    "TEST_TABLE(" +
                    "TEST_VAL1 text," +
                    "TEST_VAL2 integer" +
                    ")";

                command.ExecuteNonQuery();

            }
        }

    }
}
