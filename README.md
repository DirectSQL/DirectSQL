# DirectSQL
[![build on master](https://7k8m.visualstudio.com/DirectSQL/_apis/build/status/DirectSQL-.NET-CI)](https://7k8m.visualstudio.com/DirectSQL/_build?definitionId=1)(7k8m)<br/>
[![Build Status](https://dev.azure.com/tmnk/DirectSQL/_apis/build/status/7k8m.DirectSQL?branchName=master)](https://dev.azure.com/tmnk/DirectSQL/_build/latest?definitionId=2&branchName=master)(tmnk)<br>
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
