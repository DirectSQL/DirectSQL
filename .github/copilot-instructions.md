# DirectSQL Copilot Instructions

## Build and test

- Build the solution from the repo root with `dotnet build --configuration Release`.
- Run the full test suite with `dotnet test --configuration Release`.
- Run a single MSTest with `dotnet test TestSqlLiteDatabase/TestSqlLiteDatabase.csproj --filter FullyQualifiedName~TestSqlLiteDatabase.QueryTest.TestQuery`.
- The projects target `net8.0`. CI uses .NET 8.0.100, and `global.json` allows rolling forward from 7.0.100 to a newer major SDK.
- `DirectSQL/DirectSQL.csproj` has `GeneratePackageOnBuild=true`, so builds also produce a NuGet package in `DirectSQL/bin/<Configuration>/`.
- In the current dependency set, SQLite tests fail on macOS before assertions run because `System.Data.SQLite.Core` cannot load `SQLite.Interop.dll`.

## High-level architecture

- `DirectSQL/Database.cs` is the core of the library. It owns connection lifecycle (`Process`, `ProcessAsync`), transaction wrappers, parameter creation/conversion, and the provider-agnostic SQL helpers (`ExecuteNonQuery`, `ExecuteScalar`, `Query`, `LoadSqlResult`, and the `FormattableString` overloads).
- Each database-specific entry point is a thin subclass of `Database<...>` under `DirectSQL/SqlLite`, `DirectSQL/SqlServer`, `DirectSQL/npgsql`, `DirectSQL/MySql`, `DirectSQL/DB2`, and `DirectSQL/Odbc`. These classes mostly capture a connection string and implement `CreateConnection()`.
- `DirectSQL/SqlResult.cs` wraps the active `IDataReader` and exposes the current row as `dynamic`, tuple arrays, typed projections, and enumerable loaders. This is the main abstraction for reading query results.
- The normal execution flow is: instantiate a concrete provider database -> enter `Process(...)` / `ProcessAsync(...)` to get an open connection -> call static query helpers on the provider type -> let `Database.cs` build the command and parameters -> read rows through `SqlResult`.
- `LoadSqlResult*` convenience APIs are thin wrappers over `Query(...)` that materialize the cursor into `dynamic[]`; they do not introduce a separate query pipeline.
- Error handling is centralized in `Database.Transaction(...)` and `Database.TransactionAsync(...)`: exceptions from the callback are wrapped in `DatabaseException`, while the low-level command and reader behavior remains straight ADO.NET.
- `TestSqlLiteDatabase` is the only test project and doubles as the main executable specification of library behavior. Most usage patterns in the README are mirrored there.

## Key conventions

- Treat the concrete `*Database` class as the entry point for connection management, then use static methods on that same concrete type inside the callback. The common pattern is `db.Process((conn, tran) => { SqlLiteDatabase.Query(...); })`, not instance methods for query execution.
- Transaction callbacks are manual-commit. `Process((conn, tran) => ...)` and `Database.Transaction(...)` provide a transaction object, but the library does not commit for you; tests that need persistence call `tran.Commit()` explicitly and use `tran.Rollback()` explicitly for rollback scenarios. The library only rolls back automatically when it catches an exception and rethrows `DatabaseException`.
- `SqlResult` is cursor-based. Call `result.Next()` before reading `ResultValues` or `ResultTuples`; attempting to read row values before the first `Next()` or after the cursor is exhausted raises the underlying reader error. `ResultValues` and `ResultTuples` are cached for the current row and reset on the next `Next()`.
- Prefer the built-in parameter helpers over handwritten command setup. The codebase uses tuple parameters like `new (string, object)[] { ("@val1", value) }` and `CreateParameter(...)` when an explicit `DbType` is needed.
- Prefer the `FormattableString` helpers when authoring inline SQL in code. `ExecuteFormattableNonQuery`, `ExecuteFormattableScalar`, `QueryFormattable`, and `LoadFormattableSqlResult` rewrite interpolated values into generated bound parameters like `@0`, `@1`, and tests assert against that rewritten SQL.
- The async APIs are wrappers around synchronous ADO.NET work via `Task.Run`, not provider-native async I/O. Keep that in mind when changing behavior or adding new async surfaces.
