using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data;
using DirectSQL.SqlLite;

using System.Threading.Tasks;

namespace TestSqlLiteDatabase
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public void TestConnect()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            db.Process((connection) => {
            });

            db.Process( (connection, transaction) =>
            {
            });
        }

        [TestMethod]
        public void TestConnectAsync()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            Task task1 = db.ProcessAsync(async (connection) =>
            {
            });
            task1.Wait();

            Task task2 = db.ProcessAsync( async (connection, transaction) =>
            {
            });
            task2.Wait();

        }

    }
}
