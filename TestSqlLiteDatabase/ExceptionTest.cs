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
    }
}