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

            db
            .Process((connection) => {})
            .Process((connection, transaction) => {} );
        }

        [TestMethod]
        public async Task TestConnectAsync()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");

            await 
            ( await db.ProcessAsync(
                async (connection) => {
                    await Task.Delay(1); //Dummy code to resolve warning in build.
                })
            )
            .ProcessAsync( 
                async (connection, transaction) =>{
                    await Task.Delay(1); //Dummy code to resolve warning in build.
            });
        }
    }
}
