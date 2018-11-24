using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;

using DirectSQL;
using DirectSQL.SqlLite;
using System.Data;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class TestSqlResult
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

                Database.ExecuteNonQuery(
                    "insert into TEST_TABLE(TEST_COL1,TEST_COL2) " +
                    "VALUES(@testVal1,@testVal2)",
                    new (string, object)[] {
                            ("@testVal1","testValue"),
                            ("testVal2",123) },
                    conn,
                    tran);

                Database.Query(
                    "select TEST_COL1,TEST_COL2 from TEST_TABLE", 
                    conn, 
                    tran, 
                    (result) => {
                        result.Next();
                        var fieldNames = result.ResultFields;
                        Assert.AreEqual(fieldNames[0], "TEST_COL1");
                        Assert.AreEqual(fieldNames[1], "TEST_COL2");
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


    }
}
