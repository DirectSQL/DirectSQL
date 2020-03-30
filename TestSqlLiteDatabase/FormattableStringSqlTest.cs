using DirectSQL.SqlLite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;

namespace TestSqlLiteDatabase
{
    [TestClass] 
    public class FormattableStringSqlTest
    {
        [TestMethod] public void TestNonQuery()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn,tran) => {
                SqlLiteDatabase.ExecuteNonQuery("create table TEST_TABLE(COL1 text)",conn,tran);

                var testValue = "TESTVALUE";
                SqlLiteDatabase.ExecuteFormattableNonQuery($"insert into TEST_TABLE(COL1) values({testValue})", conn, tran);

                Assert.AreEqual(testValue, SqlLiteDatabase.ExecuteScalar("select COL1 from TEST_TABLE", conn, tran));

            });
        }

        [TestMethod]
        public void TestScalar ()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) => {
                var testValue = "TESTVALUE";
                var result = SqlLiteDatabase.ExecuteFormattableScalar($"values({testValue})", conn, tran);

                Assert.AreEqual(testValue, result);
            });
        }

        [TestMethod]
        public void TestQuery()
        {
            SqlLiteDatabase db = new SqlLiteDatabase("Data Source=:memory:");
            db.Process((conn, tran) => {
                var testValue = "TESTVALUE";
                var testValue2 = "TESTVALUE2";

                SqlLiteDatabase.QueryFormattable(
                    $"values({testValue},{testValue2})", 
                    conn, 
                    tran,
                    (result) => {
                        Assert.AreEqual(result.Sql, "values(@0,@1)");

                        result.Next();
                        Assert.AreEqual(testValue, result.ResultTuples[0].value);
                        Assert.AreEqual(testValue2, result.ResultTuples[1].value);
                    }
                );
            });
        }
    }
}
