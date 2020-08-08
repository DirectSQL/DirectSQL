# What is this for ?
[![Build Status](https://github.com/DirectSQL/DirectSQL/workflows/.NET%20Core/badge.svg?branch=main)](https://github.com/DirectSQL/DirectSQL/actions)<br/>
[![CodeFactor](https://www.codefactor.io/repository/github/directsql/directsql/badge/main)](https://www.codefactor.io/repository/github/directsql/directsql/overview/main)<br/>
[![Ready on NuGet](https://img.shields.io/nuget/v/DirectSQL.svg?style=flat)](http://NuGet.org/packages/DirectSQL/)

Execute SQL directly !

This .NET Core library is very thin database framework on top of System.Data.
In this library, you can use SQL directly without taking care for lifecycle of connection, transaction and so on.

In many database framework, direct execution of SQL is more or less barriered from orthodox.
This library resolve issues around that.

Please try :smile:.

# Where to start
This library prepare corresponding Database class for supporting RDBMS
* DirectSQL.SqlLite.SqlLiteDatabase (SQLite)
* DirectSQL.SqlServer.SqlServerDatabase (SQL Server)
* DirectSQL.npgsql.NpgsqlDatabase (PostgreSQL, CockroachDB)
* DirectSQL.DB2.DB2Database (DB2)
* DirectSQL.MySql.MySqlDatabase (MySql)
* DirectSQL.Odbc.OdbcDatabase (ODBC connected DBMS)

These Database classes are entry point of this library.

# Documentation
Documentation is [here](https://directsql.github.io/DirectSQL.Document/doc/).

# Examples
## Example1
```
public static void example()
{
    SqlLiteDatabase db = new SqlLiteDatabase("connectionString_to_yourdb");
    db.Process((connection) =>
    {
        SqlLiteDatabase.Query(
            "select TEST_VAL1,TEST_VAL2 from TEST_TABLE where TEST_VAL1 = @val1",
            new (String, object)[] {("@val1","abcdef")},
            connection,
            (result) => {
                while (result.Next())
                {
                    var resultValues = result.ResultValues;
                    Console.Out.WriteLine("TEST_VAL1:" + resultValues.TEST_VAL1);
                    Console.Out.WriteLine("TEST_VAL2:" + resultValues.TEST_VAL2);
                }
            }
        );
    });
}
```

## Example2
```
public static void example2()
{
    SqlLiteDatabase db = new SqlLiteDatabase("connectionString_to_yourdb");
    db.Process((connection) =>
    {
        dynamic[] resultArray =
            SqlLiteDatabase
            .LoadSqlResult(
                "select TEST_COL1,TEST_COL2 from TEST_TABLE",
                connection);
        
        Console.Out.WriteLine("TEST_VAL1:" + resultArray[0].TEST_VAL1);
        Console.Out.WriteLine("TEST_VAL2:" + resultArray[0].TEST_VAL2);
    });
}
```
## Refer [code in test project](https://github.com/DirectSQL/DirectSQL/tree/master/TestSqlLiteDatabase), also
# How to build and test
````
 git clone git@github.com:DirectSQL/DirectSQL.git
 cd DirectSQL
 dotnet build
 dotnet test
````
