using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DirectSQL
{
    /// <summary>
    /// Execute a sql with a connection and a transaction.
    /// </summary>
    /// <param name="connection">a connection</param>
    /// <param name="transaction">a transaction</param>
    public delegate void SqlExecution(IDbConnection connection, IDbTransaction transaction);

    /// <summary>
    /// Execute a sql asynchronously with a connection and a transaction.
    /// </summary>
    /// <param name="connection">a connection</param>
    /// <param name="transaction">a transaction</param>
    /// <returns></returns>
    public delegate Task AsyncSqlExecution(IDbConnection connection, IDbTransaction transaction);

    /// <summary>
    /// Do something with a connection
    /// </summary>
    /// <param name="connection">a connection</param>
    public delegate void ConnectExecution(IDbConnection connection);

    /// <summary>
    /// Do something asynchronously with a connection.
    /// </summary>
    /// <param name="connection">a connection</param>
    /// <returns></returns>
    public delegate Task AsyncConnectExecution(IDbConnection connection);

    /// <summary>
    /// Read sql result.
    /// </summary>
    /// <param name="result">result to be read</param>
    public delegate void ReadSqlResult(SqlResult result);

    /// <summary>
    /// Database class is entry point of DirectSQL library.
    /// </summary>
    public abstract class Database
    {

        /// <summary>
        /// Asynchronous process with a connection
        /// </summary>
        /// <param name="execute">execution with a connection</param>
        /// <returns></returns>
        public async Task ProcessAsync(AsyncConnectExecution execute)
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
        public async Task ProcessAsync(AsyncSqlExecution execute)
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
        public void Process(ConnectExecution execute)
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
        public void Process(SqlExecution execute)
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
        protected abstract IDbConnection CreateConnection();

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
            IDbConnection connection,
            IDbTransaction transaction)
        {
            using( var command = connection.CreateCommand())
            {
                command.Transaction = transaction;

                command.CommandText = sql;
                SetParameters(command, parameters);

                return command.ExecuteNonQuery();
            }
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
            IDbConnection connection,
            IDbTransaction transaction)
        {
            return ExecuteNonQuery(sql, new ValueTuple<String, object>[0], connection, transaction);
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
            IDbConnection connection,
            IDbTransaction transaction)
        {
            return ExecuteScalar(sql, new ValueTuple<String, object>[0], connection, transaction);
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
            IDbConnection connection,
            IDbTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;

                command.CommandText = sql;
                SetParameters(command, parameters);

                return command.ExecuteScalar();
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
            IDbConnection connection, 
            IDbTransaction transaction,
            ReadSqlResult readResult)
        {
            using (var result = new SqlResult(sql,parameters,connection,transaction))
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
            IDbConnection connection,
            IDbTransaction transaction,
            ReadSqlResult readResult)
        {
            Query(sql, new (String,object)[0],connection,transaction, readResult);
        }

        /// <summary>
        /// Bind parameters to a command of sql
        /// </summary>
        /// <param name="command">command of sql</param>
        /// <param name="parameters">parameter values bound to sql</param>
        internal static void SetParameters(IDbCommand command, (String name,object value)[] parameters)
        {
            for (int i = 0; i < parameters.Length; i ++)
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = parameters[i].name;
                parameter.Value = parameters[i].value;

                command.Parameters.Add(parameter);
            }
        }


        /// <summary>
        /// Execute in a transaction.
        /// </summary>
        /// <param name="connection">connection to be used</param>
        /// <param name="execute">to be executed</param>
        public static void Transaction(IDbConnection connection, SqlExecution execute)
        {
            using(var transaction = connection.BeginTransaction())
            {
                try
                {
                    execute(connection, transaction);
                }
                catch( Exception exception )
                {
                    transaction.Rollback();
                    throw new DatabaseException( MessageResource.msg_error_sqlExecutionError, exception );
                }
            }
        }


        /// <summary>
        /// Execute in a transaction asynchronously.
        /// </summary>
        /// <param name="connection">connection to be used</param>
        /// <param name="execute">to be executed</param>
        /// <returns>A task stands for asynchronous execution</returns>
        public static async Task TransactionAsync(IDbConnection connection, AsyncSqlExecution execute)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    await execute(connection, transaction);
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    throw new DatabaseException(MessageResource.msg_error_sqlExecutionError, exception);
                }
            }
        }


    }
}
