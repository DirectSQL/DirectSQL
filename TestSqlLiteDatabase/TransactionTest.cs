using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using DirectSQL;
using DirectSQL.SqlLite;
using System.Threading.Tasks;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class TransactionTest
    {
        [TestMethod]
        public void TestTransaction()
        {

            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            db.Process((connection) => {
                CreateTableForTest(connection);

                Database.Transaction(connection,
                    (conn, tran) => {
                        InsertDataForTest(conn, tran);
                        tran.Rollback();
                    }
                );

                Database.Transaction(connection,
                    (conn, tran) =>
                    {
                        AssertDataCount(0, conn, tran);
                        InsertDataForTest(conn, tran);
                        AssertDataCount(1, conn, tran);
                        tran.Commit();
                    }
                );

                Database.Transaction(connection,
                    (conn, tran) =>
                    {
                        AssertDataCount(1, conn, tran);
                    }
                );

            });
        }


        [TestMethod]
        public void TestTransactionAsync()
        {

            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            db.Process(async (connection) => {

                CreateTableForTest(connection);

                await Database.TransactionAsync(connection,
                    async (conn, tran) => {
                        await Task.Delay(1);
                        InsertDataForTest(conn, tran);
                        tran.Rollback();
                    }
                );

                await Database.TransactionAsync(connection,
                    async (conn, tran) =>
                    {
                        await Task.Delay(1);
                        AssertDataCount(0, conn, tran);
                        InsertDataForTest(conn, tran);
                        AssertDataCount(1, conn, tran);
                        tran.Commit();
                    }
                );

                await Database.TransactionAsync(connection,
                    async (conn, tran) =>
                    {
                        await Task.Delay(1);
                        AssertDataCount(1, conn, tran);
                    }
                );

            });
        }


        private static void AssertDataCount(long expectedCount,IDbConnection conn, IDbTransaction tran)
        {
            Assert.AreEqual(
                Database.ExecuteScalar("select count(*) from TEST_TABLE", conn, tran),
                expectedCount);
        }

        private static void CreateTableForTest(IDbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "create table " +
                    "TEST_TABLE(" +
                    "TEST_COL1 text" +
                    ")";

                command.ExecuteNonQuery();

            }
        }

        private static void InsertDataForTest(IDbConnection conn, IDbTransaction tran)
        {
            Database.ExecuteNonQuery(
                "insert into TEST_TABLE(TEST_COL1) " +
                "VALUES(@testVal1)",
                new (string, object)[] {
                    ("@testVal1","testValue")
                },
                conn,
                tran);
        }


    }
}
