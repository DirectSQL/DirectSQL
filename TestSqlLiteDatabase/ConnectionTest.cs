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
                await Task.Delay(1); //Dummy code to resolve warning in build.
            });
            task1.Wait();

            Task task2 = db.ProcessAsync( async (connection, transaction) =>
            {
                await Task.Delay(1); //Dummy code to resolve warning in build.
            });
            task2.Wait();
        }
    }
}
