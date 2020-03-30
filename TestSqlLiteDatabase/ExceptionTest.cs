using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data;
using DirectSQL.SqlLite;

using System.Threading.Tasks;
using System;
using DirectSQL;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class ExceptionTest
    {
        [TestMethod]
        public void TestException()
        {
            const String ERROR_MSG = "Intentinal Exception";
            try
            {
                SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");
                db.Process((connection, transaction) =>
                {
                    throw new ApplicationException(ERROR_MSG);
                });

                Assert.Fail();
            }
            catch (DatabaseException ex)
            {
                Assert.AreEqual(ex.InnerException.Message, ERROR_MSG);
            }
        }

        [TestMethod]
        public void TestExceptionAsync()
        {
            const String ERROR_MSG = "Intentinal Exception for test async";
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            Task task = db.ProcessAsync(async (connection, transaction) =>
            {
                await Task.Delay(1); //Dummy code to resolve warning in build.
                throw new ApplicationException(ERROR_MSG);
            });

            try
            {
                task.Wait();
                Assert.Fail();
            }
            catch (AggregateException aggregatedException)
            {
                Assert.AreEqual(
                    aggregatedException.InnerExceptions[0].InnerException.Message, //Only this happens
                    ERROR_MSG
                );
            }
        }
    }
}