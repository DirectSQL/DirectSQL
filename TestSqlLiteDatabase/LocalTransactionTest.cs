using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using DirectSQL;
using DirectSQL.SqlLite;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class LocalTransactionTest
    {
        [TestMethod]
        public void TestTransaction()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            db.Process((connection) => {
                CreateTableForTest(connection);

                SqlLiteDatabase.Transaction(connection,
                    (conn, tran) => {
                        InsertDataForTest(conn, tran);
                        tran.Rollback();
                    }
                );

                SqlLiteDatabase.Transaction(connection,
                    (conn, tran) =>
                    {
                        AssertDataCount(0, conn, tran);
                        InsertDataForTest(conn, tran);
                        AssertDataCount(1, conn, tran);
                        tran.Commit();
                    }
                );

                SqlLiteDatabase.Transaction(connection,
                    (conn, tran) =>
                    {
                        AssertDataCount(1, conn, tran);
                    }
                );
            });
        }

        [TestMethod]
        public async Task TestTransactionAsync()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            await db.ProcessAsync(async (connection) => {
                CreateTableForTest(connection);

                await SqlLiteDatabase.TransactionAsync(connection,
                    async (conn, tran) => {
                        await Task.Delay(1);
                        InsertDataForTest(conn, tran);
                        tran.Rollback();
                    }
                );

                await SqlLiteDatabase.TransactionAsync(connection,
                    async (conn, tran) =>
                    {
                        await Task.Delay(1);
                        AssertDataCount(0, conn, tran);
                        InsertDataForTest(conn, tran);
                        AssertDataCount(1, conn, tran);
                        tran.Commit();
                    }
                );

                await SqlLiteDatabase.TransactionAsync(connection,
                    async (conn, tran) =>
                    {
                        await Task.Delay(1);
                        AssertDataCount(1, conn, tran);
                    }
                );
            });
        }

        private static void AssertDataCount(long expectedCount,SQLiteConnection conn, SQLiteTransaction tran)
        {
            Assert.AreEqual(
                SqlLiteDatabase.ExecuteScalar("select count(*) from TEST_TABLE", conn, tran),
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

        private static void InsertDataForTest(SQLiteConnection conn, SQLiteTransaction tran)
        {
            SqlLiteDatabase.ExecuteNonQuery(
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
