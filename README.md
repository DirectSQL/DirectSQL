# DirectSQL
![build on master](https://7k8m.visualstudio.com/DirectSQL/_apis/build/status/DirectSQL-.NET-CI)

Execute SQL directly !

This .NET library is very thin database framework on top of System.Data.
In this library, you can use SQL directly without taking care for lifecycle of connection, transaction and so on.

In many database framework, direct execution of SQL is more or less barriered from orthodox.
This library resolve issues around that.

Now, this library has supporting code for next DBMS
* SQLite
* SQL Server
* PostgreSQL (Npgsql)
* CockroachDB (Npgsql)

Please try.
