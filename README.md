# Description
[![Build Status](https://github.com/DirectSQL/DirectSQL/workflows/.NET%20Core/badge.svg?branch=main)](https://github.com/DirectSQL/DirectSQL/actions)<br/>
[![CodeFactor](https://www.codefactor.io/repository/github/directsql/directsql/badge/main)](https://www.codefactor.io/repository/github/directsql/directsql/overview/main)<br/>
[![Ready on NuGet](https://img.shields.io/nuget/v/DirectSQL.svg?style=flat)](http://NuGet.org/packages/DirectSQL/)

Execute SQL directly !

This .NET Core library is very thin database framework on top of System.Data.
In this library, you can use SQL directly without taking care for lifecycle of connection, transaction and so on.

In many database framework, direct execution of SQL is more or less barriered from orthodox.
This library resolve issues around that.

Now, this library has supporting code for next DBMS
* SQLite
* SQL Server
* PostgreSQL (Npgsql)
* CockroachDB (Npgsql)
* DB2
* MySql
* ODBC connected DBMS

Documentation is [here](https://directsql.github.io/DirectSQL.Document/doc/).

Please try :smile:.

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
# How to Build
````
 git clone git@github.com:DirectSQL/DirectSQL.git
 cd DirectSQL
 dotnet build
````
