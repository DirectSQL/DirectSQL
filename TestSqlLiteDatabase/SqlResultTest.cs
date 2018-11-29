using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;

using DirectSQL;
using DirectSQL.SqlLite;
using System.Data;

using System.Linq;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class SqlResultTest
    {
        [TestMethod]
        public void TestCommandText()
        {
            var db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) =>
            {
                var sql = "values(1,2,3)";
                Database.Query(sql, conn, tran, (result) => {
                    Assert.AreEqual(sql, result.Sql);
                });
            });
        }

        [TestMethod]
        public void TestEmptyFieldName()
        {
            var db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) =>
            {
                var sql = "values(1,2,3)";
                Database.Query(sql, conn, tran, (result) => {
                    result.Next();
                    var fieldNames = result.ResultFields;
                });

            });
        }

        [TestMethod]
        public void TestFieldName()
        {
            var db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) =>
            {
                CreateTableForTest(conn);

                InsertDataForTest(conn, tran);

                Database.Query(
                    "select TEST_COL1,TEST_COL2 from TEST_TABLE",
                    conn,
                    tran,
                    (result) =>
                    {
                        result.Next();
                        var fieldNames = result.ResultFields;
                        Assert.AreEqual(fieldNames[0], "TEST_COL1");
                        Assert.AreEqual(fieldNames[1], "TEST_COL2");
                    });

            });
        }

        [TestMethod]
        public void TestNoCurrentRow()
        {
            var db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) =>
            {
                CreateTableForTest(conn);
                InsertDataForTest(conn, tran);

                Database.Query(
                    "select TEST_COL1,TEST_COL2 from TEST_TABLE",
                    conn,
                    tran,
                    (result) =>
                    {
                        //result.Next(); // Acutually this operation was needed.
                        Assert.ThrowsException<InvalidOperationException>(() =>
                        {
                            var failedToRead = result.ResultValues;
                        });

                        Assert.IsTrue( result.Next() );

                        var resultValues = result.ResultValues;
                        Assert.AreEqual(resultValues.TEST_COL1, "testValue");
                        Assert.AreEqual(resultValues.TEST_COL2, 123);

                        Assert.IsFalse( result.Next() );

                        Assert.ThrowsException<InvalidOperationException>(() =>
                        {
                            var failedToRead = result.ResultValues; //now empty row
                        });

                    }
                );

            });

        }


        [TestMethod]
        public void TestResultObject()
        {
            var db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) =>
            {
                CreateTableForTest(conn);

                InsertDataForTest(conn, tran);

                Database.Query(
                    "select TEST_COL1,TEST_COL2 from TEST_TABLE",
                    conn,
                    tran,
                    (result) =>
                    {
                        result.Next();
                        var resultObject =
                            result.ResultObject(
                                (original) => new SampleObject {
                                    Val1 = original.TEST_COL1,
                                    Val2 = original.TEST_COL2
                                }
                            );

                        Assert.AreEqual(resultObject.Val1, "testValue");
                        Assert.AreEqual(resultObject.Val2, 123);

                    });

            });
        }


        [TestMethod]
        public void TestEnumerable()
        {
            var db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) =>
            {
                CreateTableForTest(conn);

                InsertDataForTest(conn, tran);

                Database.Query(
                    "select TEST_COL1,TEST_COL2 from TEST_TABLE",
                    conn,
                    tran,
                    (result) =>
                    {
                        var array =
                            result.AsEnumerable<SampleObject>(
                                (original) => new SampleObject
                                {
                                    Val1 = original.TEST_COL1,
                                    Val2 = original.TEST_COL2
                                }
                            ).ToArray();

                        Assert.AreEqual(array[0].Val1, "testValue");
                        Assert.AreEqual(array[0].Val2, 123);
                        Assert.AreEqual(array.Length, 1);

                    });

            });
        }


        private static void CreateTableForTest(IDbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "create table " +
                    "TEST_TABLE(" +
                    "TEST_COL1 text," +
                    "TEST_COL2 integer" +
                    ")";

                command.ExecuteNonQuery();

            }
        }

        private static void InsertDataForTest(IDbConnection conn, IDbTransaction tran)
        {
            Database.ExecuteNonQuery(
                "insert into TEST_TABLE(TEST_COL1,TEST_COL2) " +
                "VALUES(@testVal1,@testVal2)",
                new (string, object)[] {
                    ("@testVal1","testValue"),
                    ("testVal2",123)
                },
                conn,
                tran);
        }

        public class SampleObject
        {
            public String Val1 { get; set; }
            public long Val2 { get; set; }
        }


    }
}
