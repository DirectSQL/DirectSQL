using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DirectSQL
{
    public abstract class Database
    {

        public delegate void SqlExecution(IDbConnection connection, IDbTransaction transaction);
        public delegate Task AsyncSqlExecution(IDbConnection connection, IDbTransaction transaction);

        public delegate void ConnectExecution(IDbConnection connection);
        public delegate Task AsyncConnectExecution(IDbConnection connection);

        public delegate void ReadSqlResult(SqlResult result);

        /// <summary>
        /// Asyncronous process with a connection
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
        /// Asyncronous process with a connection and a transaction
        /// </summary>
        /// <param name="execute">execution with a connection and a transaction</param>
        /// <returns></returns>
        public async Task ProcessAsync(AsyncSqlExecution execute)
        {

           await ProcessAsync(async (connection) =>
           {
               using (var transaction = connection.BeginTransaction())
               {
                   try
                   {
                       await execute(connection, transaction);

                       transaction.Commit();
                   }
                   catch (Exception exception)
                   {
                       transaction.Rollback();
                       throw new DatabaseException(MessageResource.msg_error_asyncSqlExecutionError, exception);
                   }
               }
           });

        }

        /// <summary>
        /// Syncronous process with a connection
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
        /// Syncronous process with a connection and a transaction
        /// </summary>
        /// <param name="execute">execution with a connection and a transaction</param>
        public void Process(SqlExecution execute)
        {
            Process((connection) =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        execute(connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        transaction.Rollback();
                        throw new DatabaseException(MessageResource.msg_error_sqlExecutionError, exception);
                    }
                }
            });

        }

        protected abstract IDbConnection CreateConnection();
        protected abstract IDbDataParameter CreateDbDataParameter(String name, Object value);

        public static int ExecuteNonQuery(
            string sql,
            (String,object)[] parameters,
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


        public static int ExecuteNonQuery(
            string sql,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            return ExecuteNonQuery(sql, new ValueTuple<String, object>[0], connection, transaction);
        }


        public static void Query(
            string sql,
            ValueTuple<String, object>[] parameters,  
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


        public static void Query(
            string sql,
            IDbConnection connection,
            IDbTransaction transaction,
            ReadSqlResult readResult)
        {
            Query(sql, new ValueTuple<String,object>[0],connection,transaction, readResult);
        }

        internal static void SetParameters(IDbCommand command, ValueTuple<String,object>[] parameters)
        {
            for (int i = 0; i < parameters.Length; i ++)
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = parameters[i].Item1;
                parameter.Value = parameters[i].Item2;

                command.Parameters.Add(parameter);
            }
        }

    }
}
