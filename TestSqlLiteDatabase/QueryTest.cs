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
                SqlLiteDatabase.ExecuteNonQuery(
                     "insert into TEST_TABLE(TEST_VAL1,TEST_VAL2) values('xyz',456)",
                     connection,
                     transaction);

                SqlLiteDatabase.Query(
                    "select TEST_VAL1,TEST_VAL2 from TEST_TABLE",
                    connection,
                    transaction,
                    (result) => {
                        if (result.Next())
                        {
                            var resultValues = result.ResultValues;
                            Assert.AreEqual(resultValues.TEST_VAL1, "abcdef");
                            Assert.AreEqual(resultValues.TEST_VAL2, 123L);

                            //Same instance
                            Assert.AreSame(resultValues, result.ResultValues);

                            var resultTuples = result.ResultTuples;
                            Assert.AreEqual(resultTuples[0].name, "TEST_VAL1");
                            Assert.AreEqual(resultTuples[0].value, "abcdef");
                            Assert.AreEqual(resultTuples[1].name, "TEST_VAL2");
                            Assert.AreEqual(resultTuples[1].value, 123L); 

                            //go to next row.
                            result.Next();

                            var resultValues2 = result.ResultValues;
                            Assert.AreEqual(resultValues2.TEST_VAL1, "xyz");
                            Assert.AreEqual(resultValues2.TEST_VAL2, 456L);
                        }
                        else
                        {
                            Assert.Fail();
                        }
                    }
                );

                SqlLiteDatabase.Query(
                    "select TEST_VAL1,TEST_VAL2 from TEST_TABLE where TEST_VAL1 = @val1",
                    new (String, object)[] {ValueTuple.Create("@val1","abcdef")},
                    connection,
                    transaction,
                    (result) => {
                        if (result.Next())
                        {
                            var resultValues = result.ResultValues;
                            Assert.AreEqual(resultValues.TEST_VAL1, "abcdef");
                            Assert.AreEqual(resultValues.TEST_VAL2, 123L);
                        }
                        else
                        {
                            Assert.Fail();
                        }
                    }
                );

                SqlLiteDatabase.Query(
                    "select TEST_VAL1,TEST_VAL2 from TEST_TABLE where TEST_VAL1 = @val1",
                    new (String, object)[] { ValueTuple.Create("@val1", "XXXXXXX") },
                    connection,
                    transaction,
                    (result) => {
                        Assert.IsFalse(result.Next());
                    }
                );

                var abcdef = SqlLiteDatabase.ExecuteScalar(
                    "select TEST_VAL1 from TEST_TABLE",
                    connection,
                    transaction
                );
                Assert.AreEqual(abcdef, "abcdef");

                var abcdef2 = SqlLiteDatabase.ExecuteScalar(
                    "select TEST_VAL1 from TEST_TABLE where TEST_VAL1 = @val1",
                    new (String, object)[] { ValueTuple.Create("@val1", "abcdef") },
                    connection,
                    transaction
                );
                Assert.AreEqual(abcdef2, "abcdef");
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
