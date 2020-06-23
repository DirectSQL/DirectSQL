using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace DirectSQL
{
    /// <summary>
    /// Execute a sql with a connection and a transaction.
    /// </summary>
    /// <param name="connection">a connection</param>
    /// <param name="transaction">a transaction</param>
    /// <typeparam name="C">Type of Connection</typeparam>
    /// <typeparam name="T">Type of Transaction</typeparam>
    public delegate void SqlExecution<C,T>(C connection, T transaction) 
        where C : IDbConnection 
        where T : IDbTransaction;

    /// <summary>
    /// Execute a sql asynchronously with a connection and a transaction.
    /// </summary>
    /// <param name="connection">a connection</param>
    /// <param name="transaction">a transaction</param>
    /// <typeparam name="C">Type of Connection</typeparam>
    /// <typeparam name="T">Type of Transaction</typeparam>
    /// <returns>Task of asynchronous process</returns>
    public delegate Task AsyncSqlExecution<C,T>(C connection, T transaction) 
        where C : IDbConnection 
        where T : IDbTransaction;

    /// <summary>
    /// Do something with a connection
    /// </summary>
    /// <param name="connection">a connection</param>
    /// <typeparam name="C">Type of Connection</typeparam>
    public delegate void ConnectExecution<C>(C connection) 
        where C : IDbConnection;

    /// <summary>
    /// Do something asynchronously with a connection.
    /// </summary>
    /// <param name="connection">a connection</param>
    /// <typeparam name="C">Type of Connection</typeparam>
    /// <returns>Task of asynchronous process</returns>
    public delegate Task AsyncConnectExecution<C>(C connection) 
        where C : IDbConnection;

    /// <summary>
    /// Read sql result.
    /// </summary>
    /// <param name="result">result to be read</param>
    /// <typeparam name="R">Type of DataReader</typeparam>
    /// <typeparam name="CMD">Type of DbCommand</typeparam>
    /// <typeparam name="T">Type of Transaction</typeparam>
    /// <typeparam name="C">Type of Connection</typeparam>
    /// <typeparam name="P">Type of DataParameter</typeparam>
    public delegate void ReadSqlResult<R,CMD,T,C,P>(SqlResult<R,CMD,T,C,P> result)  
        where CMD : IDbCommand 
        where R : IDataReader 
        where T : IDbTransaction 
        where C : IDbConnection 
        where P : IDataParameter,
        new();

    /// <summary>
    /// Database class is entry point of DirectSQL library.
    /// </summary>
    /// <typeparam name="C">Type of Connection</typeparam>
    /// <typeparam name="T">Type of Transaction</typeparam>
    /// <typeparam name="CMD">Type of DbCommand</typeparam>
    /// <typeparam name="R">Type of DataReader</typeparam>
    /// <typeparam name="P">Type of DataParameter</typeparam>
    public abstract class Database<C,T,CMD,R,P> 
        where C : IDbConnection 
        where T : IDbTransaction 
        where CMD : IDbCommand 
        where R : IDataReader 
        where P : IDataParameter,
        new()
    {
        /// <summary>
        /// Asynchronous process with a connection
        /// </summary>
        /// <param name="execute">execution with a connection</param>
        /// <returns></returns>
        public async Task ProcessAsync(AsyncConnectExecution<C> execute)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                await execute(connection);
            }
        }

        /// <summary>
        /// Asynchronous process with a connection and a transaction
        /// </summary>
        /// <param name="execute">execution with a connection and a transaction</param>
        /// <returns></returns>
        public async Task ProcessAsync(AsyncSqlExecution<C,T> execute)
        {
           await ProcessAsync(async (connection) =>
           {
               await TransactionAsync(connection, execute);
           });
        }

        /// <summary>
        /// Synchronous process with a connection
        /// </summary>
        /// <param name="execute">execution with a connection</param>
        public void Process(ConnectExecution<C> execute)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                execute(connection);
            }
        }

        /// <summary>
        /// Synchronous process with a connection and a transaction
        /// </summary>
        /// <param name="execute">execution with a connection and a transaction</param>
        public void Process(SqlExecution<C,T> execute)
        {
            Process((connection) =>
            {
                Transaction(connection, execute);
            });
        }

        /// <summary>
        /// Sub class implements actual method to create a connection.
        /// </summary>
        /// <returns></returns>
        protected abstract C CreateConnection();

        /// <summary>
        /// Execute a sql like update / insert
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">
        ///  transaction to execute sql. 
        ///  If value is defaultTransaction, this value is not used and default value in command is used.</param>
        /// <returns>affected row count</returns>
        private static int ExecuteNonQueryCore(
            string sql,
            P[] parameters,
            C connection,
            Object transaction)
        {
            using( var command = (CMD) connection.CreateCommand())
            {                
                if( transaction != defaultTransaction)
                    command.Transaction = (T) transaction;

                command.CommandText = sql;
                foreach( var param in parameters ){
                    command.Parameters.Add(param);
                }

                return command.ExecuteNonQuery();
            }
        }

        /// Execute a sql like update / insert
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql.</param>
        /// <returns>affected row count</returns>
        private static int ExecuteNonQuery(
            string sql,
            P[] parameters,
            C connection,
            T transaction)
        {
            return ExecuteNonQueryCore(sql,parameters,connection,transaction);
        }

        /// Execute a sql like update / insert
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <returns>affected row count</returns>
        private static int ExecuteNonQuery(
            string sql,
            P[] parameters,
            C connection)
        {
            return ExecuteNonQueryCore(
                sql,
                parameters,
                connection,
                defaultTransaction);
        }

        /// <summary>
        /// Execute a sql like update / insert
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>affected row count</returns>
        public static int ExecuteNonQuery(
            string sql,
            (String name, object value)[] parameters,
            C connection,
            T transaction)
        {
            return ExecuteNonQueryCore(
                sql,
                ConvertToDbParameter(parameters),
                connection,
                transaction
            );
        }

        /// <summary>
        /// Execute a sql like update / insert
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <returns>affected row count</returns>
        public static int ExecuteNonQuery(
            string sql,
            (String name, object value)[] parameters,
            C connection
        )
        {
            return ExecuteNonQueryCore(
                sql,
                ConvertToDbParameter(parameters),
                connection,
                defaultTransaction
            );
        }

        /// <summary>
        /// Execute a sql like update / insert
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>affected row count</returns>
        public static int ExecuteNonQuery(
            string sql,
            C connection,
            T transaction
        )
        {
            return ExecuteNonQuery(
                sql, 
                new ValueTuple<String, object>[0], 
                connection, 
                transaction
            );
        }

        /// <summary>
        /// Execute a sql like update / insert
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>affected row count</returns>
        public static int ExecuteNonQuery(
            string sql,
            C connection
        )
        {
            return ExecuteNonQuery(
                sql, 
                new ValueTuple<String, object>[0], 
                connection
            );
        }

        /// <summary>
        /// Execute a sql like update / insert
        /// </summary>
        /// <remarks>
        /// Formattable string is handled as sql with bind parameters
        /// </remarks>
        /// <param name="sql">sql to execute in formattable string</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>affected row count</returns>
        public static int ExecuteFormattableNonQuery(
            FormattableString sql,
            C connection,
            T transaction)
        {
            var sqlAndParams = ExtractSqlAndParam(sql);
            return ExecuteNonQuery(
                sqlAndParams.sql,
                sqlAndParams.parameters,
                connection,
                transaction);
        }

        /// <summary>
        /// Execute a sql like update / insert
        /// </summary>
        /// <remarks>
        /// Formattable string is handled as sql with bind parameters
        /// </remarks>
        /// <param name="sql">sql to execute in formattable string</param>
        /// <param name="connection">connection to execute sql</param>
        /// <returns>affected row count</returns>
        public static int ExecuteFormattableNonQuery(
            FormattableString sql,
            C connection
        )
        {
            var sqlAndParams = ExtractSqlAndParam(sql);
            return ExecuteNonQuery(
                sqlAndParams.sql,
                sqlAndParams.parameters,
                connection);
        }

        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteScalarCore(
            string sql,
            P[] parameters,
            C connection,
            Object transaction)
        {
            using (var command = (CMD) connection.CreateCommand())
            {
                if( transaction != defaultTransaction)
                {
                    command.Transaction = (T)transaction;
                }

                command.CommandText = sql;
                
                foreach(var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                return command.ExecuteScalar();
            }
        }
        
        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteScalar(
            string sql,
            C connection,
            T transaction)
        {
            return ExecuteScalar(
                sql, 
                new ValueTuple<String, object>[0], 
                connection, 
                transaction
            );
        }

        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteScalar(
            string sql,
            C connection)
        {
            return ExecuteScalarCore(
                sql, 
                new P[0], 
                connection, 
                defaultTransaction
            );
        }

        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteScalar(
            string sql,
            P[] parameters,
            C connection,
            T transaction)
        {
            return ExecuteScalarCore(sql,parameters,connection,transaction);
        }

        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteScalar(
            string sql,
            P[] parameters,
            C connection)
        {
            return ExecuteScalarCore(sql,parameters,connection,defaultTransaction);
        }

        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteScalar(
            string sql,
            (String name, object value)[] parameters,
            C connection,
            T transaction)
        {
            return ExecuteScalar(
                sql,
                ConvertToDbParameter(parameters),
                connection,
                transaction
            );
        }
        /// <summary>
        
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">connection to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteScalar(
            string sql,
            (String name, object value)[] parameters,
            C connection)
        {
            return ExecuteScalarCore(
                sql,
                ConvertToDbParameter(parameters),
                connection,
                defaultTransaction
            );
        }

        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteFormattableScalar(
            FormattableString sql,
            C connection,
            T transaction)
        {
            var extracted = ExtractSqlAndParam(sql);
            return ExecuteScalar(
                extracted.sql, 
                extracted.parameters, 
                connection, 
                transaction
            );
        }

        /// <summary>
        /// Execute a sql and get a scalar.
        /// </summary>
        /// <param name="sql">sql to execute</param>
        /// <param name="connection">connection to execute sql</param>
        /// <param name="transaction">transaction to execute sql</param>
        /// <returns>result of sql</returns>
        public static object ExecuteFormattableScalar(
            FormattableString sql,
            C connection)
        {
            var extracted = ExtractSqlAndParam(sql);
            return ExecuteScalar(
                extracted.sql, 
                extracted.parameters, 
                connection
            );
        }        

        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="transaction">a transaction to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void Query(
            string sql,
            P[] parameters,  
            C connection, 
            T transaction,
            ReadSqlResult<R,CMD,T,C,P> readResult)
        {
            using (var result = 
                    new SqlResult<R,CMD,T,C,P>(
                        sql,
                        parameters,
                        connection,
                        transaction ))
            {
                result.Init();
                readResult(result);
            }
        }

        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void Query(
            string sql,
            P[] parameters,  
            C connection, 
            ReadSqlResult<R,CMD,T,C,P> readResult)
        {
            using (var result = 
                    new SqlResult<R,CMD,T,C,P>(
                        sql,
                        parameters,
                        connection))
            {
                result.Init();
                readResult(result);
            }
        }        

        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="transaction">a transaction to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void Query(
            string sql,
            (String name, object value)[] parameters,  
            C connection, 
            T transaction,
            ReadSqlResult<R,CMD,T,C,P> readResult)
        {
            using (var result = 
                    new SqlResult<R,CMD,T,C,P>(
                        sql,
                        ConvertToDbParameter(parameters),
                        connection,
                        transaction ))
            {
                result.Init();
                readResult(result);
            }
        }

        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="parameters">parameter values bound to sql</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="transaction">a transaction to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void Query(
            string sql,
            (String name, object value)[] parameters,  
            C connection, 
            ReadSqlResult<R,CMD,T,C,P> readResult)
        {
            using (var result = 
                    new SqlResult<R,CMD,T,C,P>(
                        sql,
                        ConvertToDbParameter(parameters),
                        connection))
            {
                result.Init();
                readResult(result);
            }
        }        

        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="transaction">a transaction to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void Query(
            string sql,
            C connection,
            T transaction,
            ReadSqlResult<R,CMD,T,C,P> readResult)
        {
            Query(
                sql, 
                new (String,object)[0],
                connection,
                transaction, 
                readResult
            );
        }

        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="transaction">a transaction to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void Query(
            string sql,
            C connection,
            ReadSqlResult<R,CMD,T,C,P> readResult)
        {
            Query(
                sql, 
                new (String,object)[0],
                connection, 
                readResult
            );
        }

        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="transaction">a transaction to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void QueryFormattable(
            FormattableString sql,
            C connection,
            T transaction,
            ReadSqlResult<R,CMD,T,C,P>  readResult)
        {
            var extracted = ExtractSqlAndParam(sql);
            Query(
                extracted.sql, 
                extracted.parameters, 
                connection, 
                transaction, 
                readResult
            );
        }
        
        /// <summary>
        /// Execute a sql to query
        /// </summary>
        /// <param name="sql">a sql to query</param>
        /// <param name="connection">a connection to execute sql</param>
        /// <param name="readResult">function to read result of sql</param>
        public static void QueryFormattable(
            FormattableString sql,
            C connection,
            ReadSqlResult<R,CMD,T,C,P>  readResult)
        {
            var extracted = ExtractSqlAndParam(sql);
            Query(
                extracted.sql, 
                extracted.parameters, 
                connection, 
                readResult
            );
        }

        /// <summary>
        /// Execute in a transaction.
        /// </summary>
        /// <param name="connection">connection to be used</param>
        /// <param name="execute">to be executed</param>
        public static void Transaction(
            C connection, 
            SqlExecution<C,T> execute)
        {
            using(var transaction = (T) connection.BeginTransaction())
            {
                try
                {
                    execute(connection, transaction);
                }
                catch( Exception exception )
                {
                    transaction.Rollback();
                    throw new DatabaseException( 
                        MessageResource.msg_error_sqlExecutionError, 
                        exception 
                    );
                }
            }
        }

        /// <summary>
        /// Execute in a transaction asynchronously.
        /// </summary>
        /// <param name="connection">connection to be used</param>
        /// <param name="execute">to be executed</param>
        /// <returns>A task stands for asynchronous execution</returns>
        public static async Task TransactionAsync(
            C connection, 
            AsyncSqlExecution<C,T> execute)
        {
            using (var transaction = (T) connection.BeginTransaction())
            {
                try
                {
                    await execute(connection, transaction);
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    throw new DatabaseException(
                        MessageResource.msg_error_sqlExecutionError, 
                        exception);
                }
            }
        }

        private static(String sql, (String,Object)[] parameters) ExtractSqlAndParam(FormattableString sqlFormattableString)
        {
            var paramNames =
                Enumerable.Range(0, sqlFormattableString.ArgumentCount)
                .Select(index => $"@{index}")
                .ToArray();

            return (String.Format(sqlFormattableString.Format, paramNames), //select * from TBL where COL = @1
                    Enumerable.Range(0, sqlFormattableString.ArgumentCount)
                    .Select((index) =>
                        (paramNames[index],sqlFormattableString.GetArgument(index))
                    )
                    .ToArray() // { ("@1",value1) }
            );                
        }

        public static P CreateParameter(string name, object value)
        {
            return new P() { ParameterName = name, Value = value };
        }

        public static P CreateParameter(string name, object value, DbType type)
        {
            return new P() { ParameterName = name, Value = value, DbType = type };
        }

        public static P[] ConvertToDbParameter((String name,Object value)[] tuples)
        {
            return 
                tuples
                .Select( tp => CreateParameter(tp.name, tp.value))
                .ToArray();
        }

        public static dynamic[] LoadSqlResult(
            String sql, 
            P[] parameters,
            C connection, 
            T transaction)
        {
            return SqlResult.LoadSqlResult(sql, parameters, connection, transaction);
        }

        public static dynamic[] LoadSqlResult(
            String sql, 
            (String,object)[] parameters,
            C connection, 
            T transaction)
        {
            return SqlResult.LoadSqlResult(sql, parameters, connection, transaction);
        }

        public static dynamic[] LoadSqlResult(
            String sql,
            C connection,
            T transaction)
        {
            return SqlResult.LoadSqlResult(sql, connection, transaction);
        }

        public static dynamic[] LoadFormattableSqlResult(
            FormattableString sql, 
            C connection, 
            T transaction)
        {
            var extracted = ExtractSqlAndParam(sql);
            return SqlResult.LoadSqlResult(
                extracted.sql, 
                extracted.parameters, 
                connection, 
                transaction);
        }

        public static async Task<dynamic[]> LoadSqlResultAsync(
            String sql,
            P[] parameters,
            C connection,
            T transaction)
        {
            return await SqlResult.LoadSqlResultAsync(
                sql, 
                parameters, 
                connection, 
                transaction);
        }

        public static async Task<dynamic[]> LoadSqlResultAsync(
            String sql,
            (String, object)[] parameters,
            C connection,
            T transaction)
        {
            return await SqlResult.LoadSqlResultAsync(
                sql, 
                parameters, 
                connection, 
                transaction);
        }

        public static async Task<dynamic[]> LoadSqlResultAsync(
            String sql,
            C connection,
            T transaction)
        {
            return await SqlResult.LoadSqlResultAsync(
                sql, 
                connection, 
                transaction);
        }

        public static async Task<dynamic[]> LoadFormattableSqlResultAsync(
            FormattableString sql,
            C connection,
            T transaction)
        {
            var extracted = ExtractSqlAndParam(sql);
            return await SqlResult.LoadSqlResultAsync(
                extracted.sql,
                extracted.parameters,
                connection,
                transaction);
        }

        //Marker object indicates default transaction is used.
        internal static readonly Object defaultTransaction = new Object();

        public class SqlResult: DirectSQL.SqlResult<R,CMD,T,C,P>
        {
            internal SqlResult(
                String sql,
                P[] parameters,
                C connection,
                T transaction) : base( sql, parameters, connection, transaction )
            {
            }
        }
    }
}
