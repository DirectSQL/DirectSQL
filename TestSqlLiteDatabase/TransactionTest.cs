using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using DirectSQL;
using DirectSQL.SqlLite;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Transactions;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class TransactionTest
    {
        [TestMethod]
        public void TestTransaction()
        {
            SqlLiteDatabase db = 
                new SqlLiteDatabase(RandomNameMemDbConnectionString());
            db.Process(CreateTableForTest);

            try
            {
                using (var scope = new TransactionScope())
                {
                    db.Process(InsertDataForTest);
                }
            }
            catch (TransactionAbortedException)
            {
                //Ignore  here
            }

            using (var scope = new TransactionScope())
            {
                db.Process(
                    (conn) =>
                    {
                        AssertDataCount(0, conn);
                        InsertDataForTest(conn);
                        AssertDataCount(1, conn);
                    }
                );
                scope.Complete();
            }

            db.Process(
                (conn) => { AssertDataCount(1, conn); }
            );
        }

        [TestMethod]
        public async Task TestTransactionAsync()
        {
            SqlLiteDatabase db = 
                new SqlLiteDatabase(RandomNameMemDbConnectionString());

            await db.ProcessAsync(async (connection) => { 
                await Task.Delay(1);
                CreateTableForTest(connection); 
            });

            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await db.ProcessAsync(
                        async (conn) =>
                        {
                            await Task.Delay(1);
                            InsertDataForTest(conn);
                        }
                    );
                }
            }
            catch (TransactionAbortedException)
            {
                //Ignore
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await db.ProcessAsync(
                    async (conn) =>
                    {
                        await Task.Delay(1);
                        AssertDataCount(0, conn);
                        InsertDataForTest(conn);
                        AssertDataCount(1, conn);
                    }
                );

                scope.Complete();
            }

            await db.ProcessAsync(
                async (conn) =>
                {
                    await Task.Delay(1);
                    AssertDataCount(1, conn);
                }
            );
        }

        private static void AssertDataCount(long expectedCount,SQLiteConnection conn)
        {
            Assert.AreEqual(
                SqlLiteDatabase.ExecuteScalar("select count(*) from TEST_TABLE", conn),
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

        private static void InsertDataForTest(SQLiteConnection conn)
        {
            SqlLiteDatabase.ExecuteNonQuery(
                "insert into TEST_TABLE(TEST_COL1) " +
                "VALUES(@testVal1)",
                new (string, object)[] {
                    ("@testVal1","testValue")
                },
                conn);
        }

        private static string RandomNameMemDbConnectionString()
        {
            return $"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared";
        }
    }
}
